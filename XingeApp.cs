using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;

namespace XingeApp
{
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

        public static int IOSENV_PROD = 1;
        public static int IOSENV_DEV = 2;

        public static long IOS_MIN_ID = 2200000000L;

        private long m_accessId;
        private string m_secretKey;

        public XingeApp(long accessID, string secretKey)
        {
            this.m_accessId = accessID;
            this.m_secretKey = secretKey;
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
            if (this.m_accessId > IOS_MIN_ID)
                return token.Length == 64;
            else
                return (token.Length == 40 || token.Length == 64);
        }

        protected Boolean isValidMessageType(Message msg)
        {
            if (this.m_accessId < IOS_MIN_ID)
                return true;
            else
                return false;
        }

        protected Boolean isValidMessageType(MessageIOS message, int environment)
        {
            if (this.m_accessId >= IOS_MIN_ID && (environment == IOSENV_PROD || environment == IOSENV_DEV))
                return true;
            else
                return false;
        }

        protected Dictionary<string, object> initParams()
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            DateTime Epoch = new DateTime(1970, 1, 1);
            long timestamp = (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;
            param.Add("access_id", this.m_accessId);
            param.Add("timestamp", timestamp);
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
            md5Str = method + u.Host + u.AbsolutePath + paramStr + this.m_secretKey;
            //Console.WriteLine(md5Str);
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

        public  string callRestful(string url, Dictionary<string, object> param)
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
                //HttpClient httpClient = new HttpClient();
                //HttpResponseMessage response = await httpClient.GetAsync(temp);
                //response.EnsureSuccessStatusCode();
                //string resultStr = await response.Content.ReadAsStringAsync();
                //return resultStr;
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

        //==========================================简易接口api=====================================================

        //推送给指定的设备,限Android系统使用
        public static string pushTokenAndroid(long accessId, string secretKey, string title, string content, string token)
        {
            Message message = new Message();
            message.setType(Message.TYPE_NOTIFICATION);
            message.setTitle(title);
            message.setContent(content);

            XingeApp xinge = new XingeApp(accessId, secretKey);
            string ret = xinge.PushSingleDevice(token, message);
            return (ret);
        }

        //推送给指定的设备,限IOS系统使用
        public static string pushTokenIos(long accessId, string secretKey, string content, string token, int env)
        {
            MessageIOS message = new MessageIOS();
            message.setAlert(content);
            message.setBadge(1);
            message.setSound("beep.wav");

            XingeApp xinge = new XingeApp(accessId, secretKey);
            string ret = xinge.PushSingleDevice(token, message, env);
            return (ret);
        }

        //推送给指定的账号,限Android系统使用
        public static string pushAccountAndroid(long accessId, string secretKey, string title, string content, string account)
        {
            Message message = new Message();
            message.setType(Message.TYPE_NOTIFICATION);
            message.setTitle(title);
            message.setContent(content);

            XingeApp xinge = new XingeApp(accessId, secretKey);
            string ret = xinge.PushSingleAccount(account, message);
            return (ret);
        }

        //推送给指定的账号,限IOS系统使用
        public static string pushAccountIos(long accessId, string secretKey, string content, string account, int env)
        {
            MessageIOS message = new MessageIOS();
            message.setAlert(content);
            message.setBadge(1);
            message.setSound("beep.wav");

            XingeApp xinge = new XingeApp(accessId, secretKey);
            string ret = xinge.PushSingleAccount(account, message, env);
            return (ret);
        }

        //推送给全部的设备,限Android系统使用
        public static string pushAllAndroid(long accessId, string secretKey, string title, string content)
        {
            Message message = new Message();
            message.setType(Message.TYPE_NOTIFICATION);
            message.setTitle(title);
            message.setContent(content);

            XingeApp xinge = new XingeApp(accessId, secretKey);
            string ret = xinge.PushAllDevice(message);
            return (ret);
        }

        //推送给全部的设备,限IOS系统使用
        public static string pushAllIos(long accessId, string secretKey, string content, int env)
        {
            MessageIOS message = new MessageIOS();
            message.setAlert(content);
            message.setBadge(1);
            message.setSound("beep.wav");

            XingeApp xinge = new XingeApp(accessId, secretKey);
            string ret = xinge.PushAllDevice(message, env);
            return (ret);
        }

        //推送给绑定标签的设备,限Android系统使用
        public static string pushTagAndroid(long accessId, string secretKey, string title, string content, string tag)
        {
            Message message = new Message();
            message.setType(Message.TYPE_NOTIFICATION);
            message.setTitle(title);
            message.setContent(content);

            XingeApp xinge = new XingeApp(accessId, secretKey);
            List<string> tagList = new List<string>();
            tagList.Add(tag);
            string ret = xinge.PushTags(tagList, "OR", message);
            return (ret);
        }

        //推送给绑定标签的设备,限IOS系统使用
        public static string pushTagIos(long accessId, string secretKey, string content, string tag, int env)
        {
            MessageIOS message = new MessageIOS();
            message.setAlert(content);
            message.setBadge(1);
            message.setSound("beep.wav");

            XingeApp xinge = new XingeApp(accessId, secretKey);
            List<string> tagList = new List<string>();
            tagList.Add(tag);
            string ret = xinge.PushTags(tagList, "OR", message, env);
            return (ret);
        }

        // ====================================详细接口api==========================================================

        //推送单个设备 android设备使用
        public string PushSingleDevice(string devicetoken, Message message)
        {
            if (!isValidMessageType(message))
                return "message type error!";
            if (!message.isValid())
                return "message is invalid!";
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.m_accessId);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("multi_pkg", message.getMultiPkg());
            param.Add("device_token", devicetoken);
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            string ret = callRestful(XingeApp.RESTAPI_PUSHSINGLEDEVICE, param);
            return ret;
        }

        //推送单个设备，ios设备使用,IOSENV_PROD表示生产环境，IOSENV_DEV表示开发环境
        public string PushSingleDevice(string deviceToken, MessageIOS message, int environment)
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
            param.Add("access_id", this.m_accessId);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("device_token", deviceToken);
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            param.Add("environment", environment);
            if (message.getLoopInterval() > 0 && message.getLoopTimes() > 0)
            {
                param.Add("loop_interval", message.getLoopInterval());
                param.Add("loop_times", message.getLoopTimes());
            }
            string ret = callRestful(XingeApp.RESTAPI_PUSHSINGLEDEVICE, param);
            return ret;
        }

        //推送单个账号，android设备使用
        public string PushSingleAccount(string account, Message message)
        {
            if (!isValidMessageType(message))
                return "message type error!";
            if (!message.isValid())
                return "message is invalid!";
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.m_accessId);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("multi_pkg", message.getMultiPkg());
            param.Add("account", account);
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            string ret = callRestful(XingeApp.RESTAPI_PUSHSINGLEACCOUNT, param);
            return ret;
        }

        //推送单个账号，ios设备使用
        public string PushSingleAccount(string account, MessageIOS message, int environment)
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
            param.Add("access_id", this.m_accessId);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("account", account);
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            param.Add("environment", environment);
            string ret = callRestful(XingeApp.RESTAPI_PUSHSINGLEACCOUNT, param);
            return ret;
        }

        //推送账号列表，android设备使用
        public string PushAccountList(List<string> accountList, Message message)
        {
            if (!isValidMessageType(message))
                return "message type error!";
            if (!message.isValid())
                return "message is invalid!";
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.m_accessId);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("multi_pkg", message.getMultiPkg());
            param.Add("account_list", toJArray(accountList));
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            string ret = callRestful(XingeApp.RESTAPI_PUSHACCOUNTLIST, param);
            return ret;
        }

        //推送账号列表，ios设备使用
        public string PushAccountList(List<string> accountList, MessageIOS message, int environment)
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
            param.Add("access_id", this.m_accessId);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("account_list", toJArray(accountList));
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            param.Add("environment", environment);
            string ret = callRestful(XingeApp.RESTAPI_PUSHACCOUNTLIST, param);
            return ret;
        }

        //推送全部设备，android设备使用
        public string PushAllDevice(Message message)
        {
            if (!isValidMessageType(message))
                return "message type error!";
            if (!message.isValid())
                return "message is invalid!";
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.m_accessId);
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
            string ret = callRestful(XingeApp.RESTAPI_PUSHALLDEVICE, param);
            return ret;
        }

        //推送全部设备，ios设备使用
        public string PushAllDevice(MessageIOS message, int environment)
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
            param.Add("access_id", this.m_accessId);
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
            string ret = callRestful(XingeApp.RESTAPI_PUSHALLDEVICE, param);
            return ret;
        }

        //推送标签，android设备使用
        public string PushTags(List<string> tagList, string tagOp, Message message)
        {
            if (!isValidMessageType(message))
                return "message type error!";
            if (!message.isValid() || tagList.Count == 0 || (!tagOp.Equals("AND") && !tagOp.Equals("OR")))
            {
                return "paramas invalid!";
            }
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.m_accessId);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("multi_pkg", message.getMultiPkg());
            param.Add("message_type", message.getType());
            param.Add("tags_list", toJArray(tagList));
            param.Add("tags_op", tagOp);
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            if (message.getLoopInterval() > 0 && message.getLoopTimes() > 0)
            {
                param.Add("loop_interval", message.getLoopInterval());
                param.Add("loop_times", message.getLoopTimes());
            }
            string ret = callRestful(XingeApp.RESTAPI_PUSHTAGS, param);
            return ret;
        }

        //推送标签，ios设备使用
        public string PushTags(List<string> tagList, string tagOp, MessageIOS message, int environment)
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
            param.Add("access_id", this.m_accessId);
            param.Add("expire_time", message.getExpireTime());
            param.Add("send_time", message.getSendTime());
            param.Add("message_type", message.getType());
            param.Add("tags_list", toJArray(tagList));
            param.Add("tags_op", tagOp);
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            param.Add("environment", environment);
            if (message.getLoopInterval() > 0 && message.getLoopTimes() > 0)
            {
                param.Add("loop_interval", message.getLoopInterval());
                param.Add("loop_times", message.getLoopTimes());
            }
            string ret = callRestful(XingeApp.RESTAPI_PUSHTAGS, param);
            return ret;
        }

        //创建批量推送消息，后续利用生成的pushid配合PushAccountListMultiple或PushDeviceListMultiple接口使用，限android
        public string CreateMultipush(Message message)
        {
            if (!isValidMessageType(message))
                return "message type error!";
            if (!message.isValid())
                return "message is invalid!";
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.m_accessId);
            param.Add("expire_time", message.getExpireTime());
            param.Add("multi_pkg", message.getMultiPkg());
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            string ret = callRestful(XingeApp.RESTAPI_CREATEMULTIPUSH, param);
            return ret;
        }

        //创建批量推送消息，后续利用生成的pushid配合PushAccountListMultiple或PushDeviceListMultiple接口使用，限ios
        public string CreateMultipush(MessageIOS message, int environment)
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
            param.Add("access_id", this.m_accessId);
            param.Add("expire_time", message.getExpireTime());
            param.Add("message_type", message.getType());
            param.Add("message", message.toJson());
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            param.Add("environment", environment);
            string ret = callRestful(XingeApp.RESTAPI_CREATEMULTIPUSH, param);
            return ret;
        }

        //推送消息给大批量账号
        public string PushAccountListMultiple(long pushId, List<string> accountList)
        {
            if (pushId <= 0)
                return "{'ret_code':-1,'err_msg':'pushId invalid!'}";
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.m_accessId);
            param.Add("push_id", pushId);
            param.Add("account_list", toJArray(accountList));
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            string ret = callRestful(XingeApp.RESTAPI_PUSHACCOUNTLISTMULTIPLE, param);
            return ret;
        }

        //推送消息给大批量设备
        public string PushDeviceListMultiple(long pushId, List<string> deviceList)
        {
            if (pushId <= 0)
                return "{'ret_code':-1,'err_msg':'pushId invalid!'}";
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.m_accessId);
            param.Add("push_id", pushId);
            param.Add("device_list", toJArray(deviceList));
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            string ret = callRestful(XingeApp.RESTAPI_PUSHDEVICELISTMULTIPLE, param);
            return ret;
        }

        //查询群发消息状态
        public string QueryPushStatus(List<string> pushIdList)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.m_accessId);
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
        public string QueryDeviceCount()
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.m_accessId);
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
            param.Add("access_id", this.m_accessId);
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
            param.Add("access_id", this.m_accessId);
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
            param.Add("access_id", this.m_accessId);
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
            param.Add("access_id", this.m_accessId);
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
            List<string> tag_token_pair = new List<string>();
            foreach (TagTokenPair pair in tagTokenPairs)
            {
                List<string> singleTagToken = new List<string>();
                singleTagToken.Add(pair.tag);
                singleTagToken.Add(pair.token);
                tag_token_pair.Add(singleTagToken.ToString());
            }
            param.Add("tag_token_list", toJArray(tag_token_pair));
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
            List<string> tag_token_pair = new List<string>();
            foreach (TagTokenPair pair in tagTokenPairs)
            {
                List<string> singleTagToken = new List<string>();
                singleTagToken.Add(pair.tag);
                singleTagToken.Add(pair.token);
                tag_token_pair.Add(singleTagToken.ToString());
            }
            param.Add("tag_token_list", toJArray(tag_token_pair));
            string ret = callRestful(XingeApp.RESTAPI_BATCHDELTAG, param);
            return ret;
        }

        /**
        * 查询token相关的信息，包括最近一次活跃时间，离线消息数等
        *
        * @param deviceToken 目标设备token
        */
        public string queryInfoOfToken(string deviceToken)
        {
            Dictionary < string, object > param = new Dictionary<string, object>();
            param.Add("access_id", this.m_accessId);
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
        public string queryTokensOfAccount(string account)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.m_accessId);
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
            param.Add("access_id", this.m_accessId);
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
        public string deleteAllTokensOfAccount(String account)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("access_id", this.m_accessId);
            param.Add("account", account);
            param.Add("timestamp", (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);

            string ret = callRestful(XingeApp.RESTAPI_DELETEALLTOKENSOFACCOUNT, param);
            return ret;
        }
    }
}
