using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Net.Cache;

namespace XingeApp
{
    
    /// <summery> 常量均为 API 硬编码，不可修改
    /// </summery>
    static class XGPushConstants 
    {
        public static string OrdinaryMessage = "notify";
        public static string SilentMessage = "message";
    }
    public class XingeApp
    {
        public static string RESTAPI_PUSHSINGLEDEVICE = "http://openapi.xg.qq.com/v2/push/single_device";
        public static string RESTAPI_PUSHSINGLEACCOUNT = "http://openapi.xg.qq.com/v2/push/single_account";
        public static string RESTAPI_PUSHACCOUNTLIST = "http://openapi.xg.qq.com/v2/push/account_list";
        public static string RESTAPI_PUSHALLDEVICE = "http://openapi.xg.qq.com/v2/push/all_device";
        public static string RESTAPI_PUSHTAGS = "http://openapi.xg.qq.com/v2/push/tags_device";
        public static string RESTAPI_CREATEMULTIPUSH = "http://openapi.xg.qq.com/v2/push/create_multipush";
        public static string RESTAPI_PUSHACCOUNTLISTMULTIPLE = "http://openapi.xg.qq.com/v2/push/account_list_multiple";
        public static string RESTAPI_PUSHDEVICELISTMULTIPLE = "http://openapi.xg.qq.com/v2/push/device_list_multiple";
        public static string RESTAPI_QUERYPUSHSTATUS = "http://openapi.xg.qq.com/v2/push/get_msg_status";
        public static string RESTAPI_QUERYDEVICECOUNT = "http://openapi.xg.qq.com/v2/application/get_app_device_num";
        public static string RESTAPI_QUERYTAGS = "http://openapi.xg.qq.com/v2/tags/query_app_tags";
        public static string RESTAPI_CANCELTIMINGPUSH = "http://openapi.xg.qq.com/v2/push/cancel_timing_task";
        public static string RESTAPI_BATCHSETTAG = "http://openapi.xg.qq.com/v2/tags/batch_set";
        public static string RESTAPI_BATCHDELTAG = "http://openapi.xg.qq.com/v2/tags/batch_del";
        public static string RESTAPI_QUERYTOKENTAGS = "http://openapi.xg.qq.com/v2/tags/query_token_tags";
        public static string RESTAPI_QUERYTAGTOKENNUM = "http://openapi.xg.qq.com/v2/tags/query_tag_token_num";
        public static string RESTAPI_QUERYINFOOFTOKEN = "http://openapi.xg.qq.com/v2/application/get_app_token_info";
        public static string RESTAPI_QUERYTOKENSOFACCOUNT = "http://openapi.xg.qq.com/v2/application/get_app_account_tokens";
        public static string RESTAPI_DELETETOKENOFACCOUNT = "http://openapi.xg.qq.com/v2/application/del_app_account_tokens";
        public static string RESTAPI_DELETEALLTOKENSOFACCOUNT = "http://openapi.xg.qq.com/v2/application/del_app_account_all_tokens";

        private static string XGPushServierHost = "https://openapi.xg.qq.com";
        private static string XGPushAppPath = "/v3/push/app";
        ///<summery> 此枚举只有在iOS平台上使用，对应于App的所处的环境
        ///</summery>
        public enum PushEnvironmentofiOS {
            product = 1,
            develop = 2
        }

        public static long iOS_MIN_ID = 2200000000L;

        private long xgPushAppAccessKey;
        private string xgPushAppSecretKey;
        //V3版本新增APP ID 字段，用来标识应用的ID
        private string xgPushAppID;

        /// <summery>对于V3版本的接口，信鸽服务器要求必须添加应用标识，即APPID，可以在前端网页的应用配置中查询
        /// <param name = "appID"> V3版本接口中对应用的标识 </param>
        /// <param name = "accessID"> V2版本接口中系统自动生成的标识 </param>
        /// <param name = "secretKey"> 用于API调用的秘钥 </param>
        /// </summery>
        public XingeApp(string appID, long accessID, string secretKey)
        {
            this.xgPushAppID        = appID;
            this.xgPushAppAccessKey = accessID;
            this.xgPushAppSecretKey = secretKey;
        }


        protected string stringToMD5(string inputString)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] encryptedBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(inputString));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                builder.AppendFormat("{0:x2}", encryptedBytes[i]);
            }
            return builder.ToString();
        }

        protected Boolean isValidToken(string token)
        {
            if (this.xgPushAppAccessKey > iOS_MIN_ID)
                return token.Length == 64;
            else
                return (token.Length == 40 || token.Length == 64);
        }

        protected Boolean isValidMessageType(Message msg)
        {
            if (this.xgPushAppAccessKey < iOS_MIN_ID)
                return true;
            else
                return false;
        }

        protected Boolean isValidMessageType(MessageiOS message, PushEnvironmentofiOS environment)
        {
            if (this.xgPushAppAccessKey >= iOS_MIN_ID && (environment == PushEnvironmentofiOS.product || environment == PushEnvironmentofiOS.develop))
                return true;
            else
                return false;
        }

        protected Dictionary<string, object> initParams()
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            // DateTime Epoch = new DateTime(1970, 1, 1);
            // long timestamp = (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;
            param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            return param;
        }

        protected JArray toJArray(List<string> strList)
        {
            JArray ja = new JArray();
            foreach (string str in strList)
            {
                ja.Add(new JValue(str));
            }
            return ja;
        }

        public string generateSign(string method, string url, Dictionary<string, object> param)
        {
            string paramStr = "";
            string md5Str = "";
            var dicSort = from objDic in param orderby objDic.Key select objDic;
            foreach (KeyValuePair<string, object> kvp in dicSort)
            {
                paramStr += kvp.Key + "=" + kvp.Value.ToString();
            }
            Uri u = new Uri(url);
            md5Str = method + u.Host + u.AbsolutePath + paramStr + this.xgPushAppSecretKey;
            md5Str = HttpUtility.UrlDecode(md5Str);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] encryptedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(md5Str));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                
                builder.Append(encryptedBytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        private string base64AuthStringOfXGPush() {
            byte[] bytes = Encoding.ASCII.GetBytes(this.xgPushAppID + ":" + this.xgPushAppSecretKey);
            return Convert.ToBase64String(bytes);
        }

        private string callRestful(string url, Dictionary<string, object> param)
        {
            string temp = "";
            string sign = generateSign("GET", url, param);
            if (sign.Length == 0)
                return "generate sign error";
            param.Add("sign", sign);
            foreach (KeyValuePair<string, object> kvp in param)
            {
                temp += kvp.Key + "=" + HttpUtility.UrlEncode(kvp.Value.ToString()) + "&";
            }

            try
            {
                temp = url + "?" + temp.Remove(temp.Length - 1, 1);
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(temp);
                
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                httpWebRequest.Timeout = 20000;
                
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                string responseContent = streamReader.ReadToEnd();
                
                httpWebResponse.Close();
                streamReader.Close();
                
                return responseContent;
            }
            catch (Exception e)
            {
                return e.ToString();
            }


        }

        protected string requestXGServerV3(string host, string path, Dictionary<string, object> param) 
        {
            HttpWebRequest request = null;
            string url = host + path;
            request = WebRequest.Create(url) as HttpWebRequest;

            request.Method = "POST";
            request.ContentType = "application/json";
            string postData = JsonConvert.SerializeObject(param);
            System.Console.WriteLine(postData);
            byte[] data = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = data.Length;

            request.Headers.Add("Authorization", "Basic " + base64AuthStringOfXGPush());
            Stream writeStream = request.GetRequestStream();
            writeStream.Write(data, 0, data.Length);
            
            
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            string result = string.Empty;
            using (StreamReader sr = new StreamReader(stream))
            {
                result = sr.ReadToEnd();
                sr.Close();
            }

            writeStream.Close();
            response.Close();

            return result;
        }

        //==========================================简易接口api=====================================================

        /// <summery> 推送普通消息给指定的设备,限Android系统使用
        /// <param name = "appID"> V3版本接口中对应用的标识 </param>
        /// <param name = "accessID"> V2版本接口中系统自动生成的标识 </param>
        /// <param name = "secretKey"> 用于API调用的秘钥 </param>
        /// <param name = "title"> 消息标题 </param>
        /// <param name = "content"> 消息内容 </param>
        /// <param name = "token"> 接收消息的设备标识 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public static string pushTokenAndroid(string appID, long accessID, string secretKey, string title, string content, string token)
        {
            Message message = new Message();
            message.setType(XGPushConstants.OrdinaryMessage);
            message.setTitle(title);
            message.setContent(content);

            XingeApp xinge = new XingeApp(appID, accessID, secretKey);
            string ret = xinge.PushSingleDevice(token, message);
            return (ret);
        }

        
        /// <summery>//推送普通消息给指定的设备,限iOS系统使用
        /// <param name = "appID"> V3版本接口中对应用的标识 </param>
        /// <param name = "accessID"> V2版本接口中系统自动生成的标识 </param>
        /// <param name = "secretKey"> 用于API调用的秘钥 </param>
        /// <param name = "content"> 消息内容 </param>
        /// <param name = "token"> 接收消息的设备标识 </param>
        /// <param name = "environment"> 指定推送环境 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public static string pushTokeniOS(string appID, long accessID, string secretKey, string content, string token, PushEnvironmentofiOS environment)
        {
            MessageiOS message = new MessageiOS();
            message.setAlert(content);

            XingeApp xinge = new XingeApp(appID, accessID, secretKey);
            string ret = xinge.PushSingleDevice(token, message, environment);
            return (ret);
        }

        /// <summery>推送普通消息给指定的账号,限Android系统使用
        /// <param name = "appID"> V3版本接口中对应用的标识 </param>
        /// <param name = "accessID"> V2版本接口中系统自动生成的标识 </param>
        /// <param name = "secretKey"> 用于API调用的秘钥 </param>
        /// <param name = "title"> 消息标题 </param>
        /// <param name = "content"> 消息内容 </param>
        /// <param name = "account"> 接收设备标识绑定的账号 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public static string pushAccountAndroid(string appID, long accessID, string secretKey, string title, string content, string account)
        {
            Message message = new Message();
            message.setType(XGPushConstants.OrdinaryMessage);
            message.setTitle(title);
            message.setContent(content);

            XingeApp xinge = new XingeApp(appID, accessID, secretKey);
            string ret = xinge.PushSingleAccount(account, message);
            return (ret);
        }

        
        /// <summery>//推送普通消息给指定的账号,限iOS系统使用
        /// <param name = "appID"> V3版本接口中对应用的标识 </param>
        /// <param name = "accessID"> V2版本接口中系统自动生成的标识 </param>
        /// <param name = "secretKey"> 用于API调用的秘钥 </param>
        /// <param name = "content"> 消息内容 </param>
        /// <param name = "account"> 接收设备标识绑定的账号 </param>
        /// <param name = "environment"> 指定推送环境 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public static string pushAccountiOS(string appID, long accessID, string secretKey, string content, string account, PushEnvironmentofiOS environment)
        {
            MessageiOS message = new MessageiOS();
            message.setAlert(content);

            XingeApp xinge = new XingeApp(appID, accessID, secretKey);
            string ret = xinge.PushSingleAccount(account, message, environment);
            return (ret);
        }

        /// <summery>推送普通消息给全部的设备,限Android系统使用
        /// <param name = "appID"> V3版本接口中对应用的标识 </param>
        /// <param name = "accessID"> V2版本接口中系统自动生成的标识 </param>
        /// <param name = "secretKey"> 用于API调用的秘钥 </param>
        /// <param name = "title"> 消息标题 </param>
        /// <param name = "content"> 消息内容 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public static string pushAllAndroid(string appID, long accessID, string secretKey, string title, string content)
        {
            Message message = new Message();
            message.setType(XGPushConstants.OrdinaryMessage);
            message.setTitle(title);
            message.setContent(content);

            XingeApp xinge = new XingeApp(appID, accessID, secretKey);
            string ret = xinge.PushAllDevice(message);
            return (ret);
        }

        /// <summery>推送普通消息给全部的设备,限iOS系统使用
        /// <param name = "appID"> V3版本接口中对应用的标识 </param>
        /// <param name = "accessID"> V2版本接口中系统自动生成的标识 </param>
        /// <param name = "secretKey"> 用于API调用的秘钥 </param>
        /// <param name = "content"> 消息内容 </param>
        /// <param name = "environment"> 指定推送环境 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public static string pushAlliOS(string appID, long accessID, string secretKey, string content, PushEnvironmentofiOS environment)
        {
            MessageiOS message = new MessageiOS();
            message.setAlert(content);

            XingeApp xinge = new XingeApp(appID, accessID, secretKey);
            string ret = xinge.PushAllDevice(message, environment);
            return (ret);
        }

        
        /// <summery>//推送普通消息给绑定标签的设备,限Android系统使用
        /// <param name = "appID"> V3版本接口中对应用的标识 </param>
        /// <param name = "accessID"> V2版本接口中系统自动生成的标识 </param>
        /// <param name = "secretKey"> 用于API调用的秘钥 </param>
        /// <param name = "title"> 消息标题 </param>
        /// <param name = "content"> 消息内容 </param>
        /// <param name = "tag"> 接收设备标识绑定的标签 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public static string pushTagAndroid(string appID, long accessID, string secretKey, string title, string content, string tag)
        {
            Message message = new Message();
            message.setType(XGPushConstants.OrdinaryMessage);
            message.setTitle(title);
            message.setContent(content);

            XingeApp xinge = new XingeApp(appID, accessID, secretKey);
            List<string> tagList = new List<string>();
            tagList.Add(tag);
            string ret = xinge.PushTags(tagList, "OR", message);
            return (ret);
        }

        /// <summery>推送普通消息给绑定标签的设备,限iOS系统使用
        /// <param name = "appID"> V3版本接口中对应用的标识 </param>
        /// <param name = "accessID"> V2版本接口中系统自动生成的标识 </param>
        /// <param name = "secretKey"> 用于API调用的秘钥 </param>
        /// <param name = "content"> 消息内容 </param>
        /// <param name = "tag"> 接收设备标识绑定的标签 </param>
        /// <param name = "environment"> 指定推送环境 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public static string pushTagiOS(string appID, long accessID, string secretKey, string content, string tag, PushEnvironmentofiOS environment)
        {
            MessageiOS message = new MessageiOS();
            message.setAlert(content);

            XingeApp xinge = new XingeApp(appID, accessID, secretKey);
            List<string> tagList = new List<string>();
            tagList.Add(tag);
            string ret = xinge.PushTags(tagList, "OR", message, environment);
            return (ret);
        }

        // ====================================详细接口api==========================================================

        /// <summery> 推送消息给指定的设备, 限Android系统使用
        /// <param name = "deviceToken"> 接收消息的设备标识 </param>
        /// <param name = "message"> Android消息结构体 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public string PushSingleDevice(string deviceToken, Message message)
        {
            if (!isValidMessageType(message))
                return "message type error!";
            if (!message.isValid())
                return "message is invalid!";
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("audience_type", "token");
            param.Add("platform", "android");

            List <string> tokenList = new List<string>();
            tokenList.Add(deviceToken);

            param.Add("token_list", toJArray(tokenList));
            // param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("multi_pkg", message.getMultiPkg());
            // param.Add("device_token", devicetoken);
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            // string ret = callRestful(XingeApp.RESTAPI_PUSHSINGLEDEVICE, param);
            string ret = requestXGServerV3(XingeApp.XGPushServierHost, XingeApp.XGPushAppPath, param);
            return ret;
        }

        /// <summery> 推送消息给多个设备, 限 Android 系统使用
        /// <param name = "deviceTokens"> 接收消息的设备标识列表 </param>
        /// <param name = "message"> Android 消息结构体 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public string PushMultipleDevices(List<string> deviceTokens, Message message)
        {
            if (!isValidMessageType(message))
                return "message type error!";
            if (!message.isValid())
                return "message is invalid!";
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("audience_type", "token_list");
            param.Add("platform", "android");
            param.Add("token_list", toJArray(deviceTokens));
            // param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("multi_pkg", message.getMultiPkg());
            // param.Add("device_token", devicetoken);
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            // string ret = callRestful(XingeApp.RESTAPI_PUSHSINGLEDEVICE, param);
            string ret = requestXGServerV3(XingeApp.XGPushServierHost, XingeApp.XGPushAppPath, param);
            return ret;
        }

        /// <summery> 推送消息给指定设备, 限 iOS 系统使用
        /// <param name = "deviceToken"> 接收消息的设备标识 </param>
        /// <param name = "message"> iOS 消息结构体 </param>
        /// <param name = "environment"> 指定推送环境 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public string PushSingleDevice(string deviceToken, MessageiOS message, PushEnvironmentofiOS environment)
        {
            if (!isValidMessageType(message, environment))
            {
                return "{'ret_code':-1,'err_msg':'message type or environment error!'}";
            }
            if (!message.isValid())
            {
                return "{'ret_code':-1,'err_msg':'message invalid!'}";
            }
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("audience_type", "token");
            param.Add("platform", "ios");

            List <string> tokenList = new List<string>();
            tokenList.Add(deviceToken);
            param.Add("token_list", toJArray(tokenList));

            // param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            // param.Add("device_token", deviceToken);
            param.Add("message_type", "notify");
            
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            param.Add("environment", environment);
            if (message.getLoopInterval() > 0 && message.getLoopTimes() > 0)
            {
                param.Add("loop_interval", message.getLoopInterval());
                param.Add("loop_times", message.getLoopTimes());
            }
            System.Console.WriteLine(param);
            string ret = requestXGServerV3(XingeApp.XGPushServierHost, XingeApp.XGPushAppPath, param);
            return ret;
        }

        /// <summery> 推送消息给多个设备, 限 iOS 系统使用
        /// <param name = "deviceTokens"> 接收消息的设备标识列表 </param>
        /// <param name = "message"> iOS 消息结构体 </param>
        /// <param name = "environment"> 指定推送环境 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public string PushMultipleDevices(List<string> deviceTokens, MessageiOS message, PushEnvironmentofiOS environment)
        {
            if (!isValidMessageType(message, environment))
            {
                return "{'ret_code':-1,'err_msg':'message type or environment error!'}";
            }
            if (!message.isValid())
            {
                return "{'ret_code':-1,'err_msg':'message invalid!'}";
            }
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("audience_type", "token_list");
            param.Add("platform", "ios");
            param.Add("token_list", toJArray(deviceTokens));
            // param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            // param.Add("device_token", deviceToken);
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            param.Add("environment", environment);
            if (message.getLoopInterval() > 0 && message.getLoopTimes() > 0)
            {
                param.Add("loop_interval", message.getLoopInterval());
                param.Add("loop_times", message.getLoopTimes());
            }
            string ret = requestXGServerV3(XingeApp.XGPushServierHost, XingeApp.XGPushAppPath, param);
            return ret;
        }

        /// <summery> 推送消息给绑定账号的设备, 限 Android 系统使用
        /// <param name = "account"> 接收设备标识绑定的账号 </param>
        /// <param name = "message"> Android 消息结构体 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public string PushSingleAccount(string account, Message message)
        {
            if (!isValidMessageType(message))
                return "message type error!";
            if (!message.isValid())
                return "message is invalid!";
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("audience_type", "account");
            param.Add("platform", "android");

            List <string> accountList = new List<string>();
            accountList.Add(account);

            param.Add("account_list", toJArray(accountList));
            // param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("multi_pkg", message.getMultiPkg());
            // param.Add("account", account);
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            string ret = requestXGServerV3(XingeApp.XGPushServierHost, XingeApp.XGPushAppPath, param);
            return ret;
        }

        /// <summery> 推送消息给绑定账号的设备, 限 iOS 系统使用
        /// <param name = "account"> 接收设备标识绑定的账号 </param>
        /// <param name = "message"> iOS 消息结构体 </param>
        /// <param name = "environment"> 指定推送环境 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public string PushSingleAccount(string account, MessageiOS message, PushEnvironmentofiOS environment)
        {
            if (!isValidMessageType(message, environment))
            {
                return "{'ret_code':-1,'err_msg':'message type or environment error!'}";
            }
            if (!message.isValid())
            {
                return "{'ret_code':-1,'err_msg':'message invalid!'}";
            }
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("audience_type", "account");
            param.Add("platform", "ios");

            List <string> accountList = new List<string>();
            accountList.Add(account);

            param.Add("account_list", toJArray(accountList));
            // param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            // param.Add("account", account);
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            param.Add("environment", environment);
            string ret = requestXGServerV3(XingeApp.XGPushServierHost, XingeApp.XGPushAppPath, param);
            return ret;
        }

        /// <summery> 推送消息给绑定账号的设备, 限 Android 系统使用
        /// <param name = "accountList"> 接收设备标识绑定的账号列表 </param>
        /// <param name = "message"> Android 消息结构体,注意：第一次推送时，message中的pushID是填写0，若需要再次推送同样的文本，需要根据返回的push_id填写 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public string PushAccountList(List<string> accountList, Message message)
        {
            if (!isValidMessageType(message))
                return "message type error!";
            if (!message.isValid())
                return "message is invalid!";
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("audience_type", "account_list");
            param.Add("platform", "android");
            // param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("multi_pkg", message.getMultiPkg());
            param.Add("account_list", toJArray(accountList));
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("push_id", message.getPushID().ToString());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            string ret = requestXGServerV3(XingeApp.XGPushServierHost, XingeApp.XGPushAppPath, param);
            return ret;
        }

        /// <summery> 推送消息给绑定账号的设备, 限 iOS 系统使用
        /// <param name = "accountList"> 接收设备标识绑定的账号列表 </param>
        /// <param name = "message"> iOS 消息结构体，注意：第一次推送时，message中的pushID是填写0，若需要再次推送同样的文本，需要根据返回的push_id填写 </param>
        /// <param name = "environment"> 指定推送环境 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public string PushAccountList(List<string> accountList, MessageiOS message, PushEnvironmentofiOS environment)
        {
            if (!isValidMessageType(message, environment))
            {
                return "{'ret_code':-1,'err_msg':'message type or environment error!'}";
            }
            if (!message.isValid())
            {
                return "{'ret_code':-1,'err_msg':'message invalid!'}";
            }
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("audience_type", "account_list");
            param.Add("platform", "ios");
            // param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("account_list", toJArray(accountList));
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("push_id", message.getPushID().ToString());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            param.Add("environment", environment);
            string ret = requestXGServerV3(XingeApp.XGPushServierHost, XingeApp.XGPushAppPath, param);
            return ret;
        }

        /// <summery> 推送消息给全部设备, 限 Android 系统使用
        /// <param name = "message"> Android 消息结构体 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public string PushAllDevice(Message message)
        {
            if (!isValidMessageType(message))
                return "message type error!";
            if (!message.isValid())
                return "message is invalid!";
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("audience_type", "all");
            param.Add("platform", "android");
            param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("multi_pkg", message.getMultiPkg());
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            if (message.getLoopInterval() > 0 && message.getLoopTimes() > 0)
            {
                param.Add("loop_interval", message.getLoopInterval());
                param.Add("loop_times", message.getLoopTimes());
            }
            string ret = requestXGServerV3(XingeApp.XGPushServierHost, XingeApp.XGPushAppPath, param);
            return ret;
        }

        /// <summery> 推送消息给全部设备, 限 iOS 系统使用
        /// <param name = "message"> iOS 消息结构体 </param>
        /// <param name = "environment"> 指定推送环境 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public string PushAllDevice(MessageiOS message, PushEnvironmentofiOS environment)
        {
            if (!isValidMessageType(message, environment))
            {
                return "{'ret_code':-1,'err_msg':'message type or environment error!'}";
            }
            if (!message.isValid())
            {
                return "{'ret_code':-1,'err_msg':'message invalid!'}";
            }
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("audience_type", "all");
            param.Add("platform", "ios");
            // param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            param.Add("environment", environment);
            if (message.getLoopInterval() > 0 && message.getLoopTimes() > 0)
            {
                param.Add("loop_interval", message.getLoopInterval());
                param.Add("loop_times", message.getLoopTimes());
            }
            string ret = requestXGServerV3(XingeApp.XGPushServierHost, XingeApp.XGPushAppPath, param);
            return ret;
        }

        /// <summery> 推送消息给绑定标签的设备, 限 Android 系统使用
        /// <param name = "accountList"> 接收设备标识绑定的标签列表 </param>
        /// <param name = "tagOp"> 标签集合需要进行的逻辑集合运算标识 </param>
        /// <param name = "message"> Android 消息结构体 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public string PushTags(List<string> tagList, string tagOp, Message message)
        {
            if (!isValidMessageType(message))
                return "message type error!";
            if (!message.isValid() || tagList.Count == 0 || (!tagOp.Equals("AND") && !tagOp.Equals("OR")))
            {
                return "paramas invalid!";
            }
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("audience_type", "tag");
            param.Add("platform", "android");
            // param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("multi_pkg", message.getMultiPkg());
            param.Add("message_type", message.getType());
            Dictionary <string, object> tagListParam = new Dictionary<string, object>();
            tagListParam.Add("tags",toJArray(tagList));
            tagListParam.Add("op", tagOp);
            param.Add("tag_list", tagListParam);
            // param.Add("tag_list", toJArray(tagList));
            // param.Add("tags_op", tagOp);
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            if (message.getLoopInterval() > 0 && message.getLoopTimes() > 0)
            {
                param.Add("loop_interval", message.getLoopInterval());
                param.Add("loop_times", message.getLoopTimes());
            }
            string ret = requestXGServerV3(XingeApp.XGPushServierHost, XingeApp.XGPushAppPath, param);
            return ret;
        }

        /// <summery> 推送消息给绑定标签的设备, 限 iOS 系统使用
        /// <param name = "accountList"> 接收设备标识绑定的标签列表 </param>
        /// <param name = "tagOp"> 标签集合需要进行的逻辑集合运算标识 </param>
        /// <param name = "message"> iOS 消息结构体 </param>
        /// <param name = "environment"> 指定推送环境 </param>
        /// <returns> 推送结果描述 </returns>
        /// </summery>
        public string PushTags(List<string> tagList, string tagOp, MessageiOS message, PushEnvironmentofiOS environment)
        {
            if (!isValidMessageType(message, environment))
            {
                return "{'ret_code':-1,'err_msg':'message type or environment error!'}";
            }
            if (!message.isValid())
            {
                return "{'ret_code':-1,'err_msg':'message invalid!'}";
            }
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("audience_type", "tag");
            param.Add("platform", "ios");
            // param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("message_type", message.getType());
            Dictionary <string, object> tagListParam = new Dictionary<string, object>();
            tagListParam.Add("tags",toJArray(tagList));
            tagListParam.Add("op", tagOp);
            param.Add("tag_list", tagListParam);
            // param.Add("tag_list", tagList);
            // param.Add("tags_op", tagOp);
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            param.Add("environment", environment);
            if (message.getLoopInterval() > 0 && message.getLoopTimes() > 0)
            {
                param.Add("loop_interval", message.getLoopInterval());
                param.Add("loop_times", message.getLoopTimes());
            }
            string ret = requestXGServerV3(XingeApp.XGPushServierHost, XingeApp.XGPushAppPath, param);
            return ret;
        }

        //查询群发消息状态
        public string QueryPushStatus(List<string> pushIdList)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            JArray ja = new JArray();
            foreach (string pushId in pushIdList)
            {
                JObject jo = new JObject();
                jo.Add("push_id", pushId);
                ja.Add(jo);
            }
            param.Add("push_ids", ja.ToString());
            string ret = callRestful(XingeApp.RESTAPI_QUERYPUSHSTATUS, param);
            return ret;
        }

        //查询消息覆盖的设备数
        private string QueryDeviceCount()
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            string ret = callRestful(XingeApp.RESTAPI_QUERYDEVICECOUNT, param);
            return ret;
        }

        /**
        * 查询应用当前所有的tags
        *
        * @param start 从哪个index开始
        * @param limit 限制结果数量，最多取多少个tag
        */
        public string QueryTags(int start, int limit)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("start", start);
            param.Add("limit", limit);
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            string ret = callRestful(XingeApp.RESTAPI_QUERYTAGS, param);
            return ret;
        }

        /**
        * 查询应用所有的tags，如果超过100个，取前100个
        *
        */
        public string QueryTags()
        {
            return QueryTags(0, 100);
        }

        /**
        * 查询带有指定tag的设备数量
        *
        * @param tag 指定的标签
        */
        public string queryTagTokenNum(string tag)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("tag", tag);
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            string ret = callRestful(XingeApp.RESTAPI_QUERYTAGTOKENNUM, param);
            return ret;
        }

        /**
        * 查询设备下所有的tag
        *
        * @param deviceToken 目标设备token
        */
        public string queryTokenTags(string deviceToken)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("device_token", deviceToken);
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);

            string ret = callRestful(XingeApp.RESTAPI_QUERYTOKENTAGS, param);
            return ret;
        }

        /**
        * 取消尚未推送的定时任务
        *
        * @param pushId 各类推送任务返回的push_id
        */
        public string cancelTimingPush(string pushId)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("push_id", pushId);
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);

            string ret = callRestful(XingeApp.RESTAPI_CANCELTIMINGPUSH, param);
            return ret;
        }

        //批量为token设置标签
        public string BatchSetTag(List<TagTokenPair> tagTokenPairs)
        {
            foreach (TagTokenPair pair in tagTokenPairs)
            {
                if(!this.isValidToken(pair.token))
                {
                    return string.Format("{\"ret_code\":-1,\"err_msg\":\"invalid token %s\"}", pair.token);
                }
            }
            Dictionary<string, object> param = this.initParams();
            List<List<string>> tag_token_pair = new List<List<string>>();
            foreach (TagTokenPair pair in tagTokenPairs)
            {
                List<string> singleTagToken = new List<string>();
                singleTagToken.Add(pair.tag);
                singleTagToken.Add(pair.token);;
                tag_token_pair.Add(singleTagToken);
            }
            var json = JsonConvert.SerializeObject(tag_token_pair);
            param.Add("tag_token_list", json);
            string ret = callRestful(XingeApp.RESTAPI_BATCHSETTAG, param);
            return ret;
        }

        //批量为token删除标签
        public string BatchDelTag(List<TagTokenPair> tagTokenPairs)
        {
            foreach (TagTokenPair pair in tagTokenPairs)
            {
                if (!this.isValidToken(pair.token))
                {
                    return string.Format("{\"ret_code\":-1,\"err_msg\":\"invalid token %s\"}", pair.token);
                }
            }
            Dictionary<string, object> param = this.initParams();
            List<List<string>> tag_token_pair = new List<List<string>>();
            foreach (TagTokenPair pair in tagTokenPairs)
            {
                List<string> singleTagToken = new List<string>();
                singleTagToken.Add(pair.tag);
                singleTagToken.Add(pair.token);
                tag_token_pair.Add(singleTagToken);
            }
            var json = JsonConvert.SerializeObject(tag_token_pair);
            param.Add("tag_token_list", json);
            string ret = callRestful(XingeApp.RESTAPI_BATCHDELTAG, param);
            return ret;
        }

        /**
        * 查询token相关的信息，包括最近一次活跃时间，离线消息数等
        *
        * @param deviceToken 目标设备token
        */
        private string queryInfoOfToken(string deviceToken)
        {
            Dictionary < string, object > param = new Dictionary<string, object>();
            param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("device_token", deviceToken);
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);

            string ret = callRestful(XingeApp.RESTAPI_QUERYINFOOFTOKEN, param);
            return ret;
        }

        /**
        * 查询账号绑定的token
        *
        * @param account 目标账号
        */
        private string queryTokensOfAccount(string account)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("account", account);
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);

            string ret = callRestful(XingeApp.RESTAPI_QUERYTOKENSOFACCOUNT, param);
            return ret;
        }

        /**
        * 删除指定账号和token的绑定关系（token仍然有效）
        *
        * @param account 目标账号
        * @param deviceToken 目标设备token
        */
        public string deleteTokenOfAccount(String account, String deviceToken)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("account", account);
            param.Add("device_token", deviceToken);
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);

            string ret = callRestful(XingeApp.RESTAPI_DELETETOKENOFACCOUNT, param);
            return ret;
        }

        /**
         * 删除指定账号绑定的所有token（token仍然有效）
         *
         * @param account 目标账号
         */
        private string deleteAllTokensOfAccount(String account)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.xgPushAppAccessKey);
            param.Add("account", account);
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);

            string ret = callRestful(XingeApp.RESTAPI_DELETEALLTOKENSOFACCOUNT, param);
            return ret;
        }
    }
}
