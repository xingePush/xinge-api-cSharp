using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XingeApp
{
    public class MessageiOS
    {
        private int m_expireTime;
        private string m_sendTime;
        private List<TimeInterval> m_acceptTimes;
        private string m_type;
        private Dictionary<string, object> m_custom;
        private string m_raw;
        private string m_alertStr;
        private Dictionary<string,object> m_alertJo;
        private int m_badge;
        private string m_sound;
        private string m_category;
        private int m_loopInterval;
        private int m_loopTimes;
        private string m_title;
        private string m_subtitle;

        private int m_pushID;
        
        /// -1，表示角标不变；
        /// -2，表示角标自动加+；
        /// n，(n >= 0)表示自定义角标数值
        private int m_badgeType;

        public MessageiOS()
        {
            this.m_sendTime = "";
            this.m_acceptTimes = new List<TimeInterval>();
            this.m_raw = "";
            this.m_alertStr = "";
            this.m_badge = 0;
            this.m_sound = "";
            this.m_category = "";
            this.m_loopInterval = -1;
            this.m_loopTimes = -1;
            this.m_type = XGPushConstants.OrdinaryMessage;
            this.m_title = "";
            this.m_subtitle = "";
            this.m_pushID = 0;
            this.m_badgeType = -1;
        }

        public void setType(string type)
        {
            this.m_type = type;
        }

        public string getType()
        {
            return m_type;
        }

        public void setExpireTime(int expireTime)
        {
            this.m_expireTime = expireTime;
        }

        public int getExpireTime()
        {
            return this.m_expireTime;
        }

        public void setSendTime(string sendTime)
        {
            this.m_sendTime = sendTime;
        }

        public string getSendTime()
        {
            return this.m_sendTime;
        }


        public void addAcceptTime(TimeInterval acceptTime)
        {
            this.m_acceptTimes.Add(acceptTime);
        }

        public JArray acceptTimeToJsonArray()
        {
            JArray json = new JArray();
            foreach (TimeInterval ti in m_acceptTimes)
            {
                JObject jtemp = JObject.FromObject(ti.toJson());
                json.Add(jtemp);
            }
            return json;
        }

        public void setCustom(Dictionary<string, object> custom)
        {
            this.m_custom = custom;
        }

        public void setRaw(string raw)
        {
            this.m_raw = raw;
        }

        public void setAlert(string alert)
        {
            m_alertStr = alert;
        }

        public void setAlert(Dictionary<string,object> alert)
        {
            m_alertJo = alert;
        }

        [Obsolete("方法已经停用，请使用setBadgeType")]
        public void setBadge(int badge)
        {
            m_badge = badge;
        }
        
        public void setTitle(string title) 
        {
            this.m_title = title;
        }
        
        public void setSubTitle(string title) 
        {
            this.m_subtitle = title;
        }

        public void setSound(string sound)
        {
            m_sound = sound;
        }

        public void setCategory(string category)
        {
            m_category = category;
        }

        public int getLoopInterval()
        {
            return m_loopInterval;
        }

        public void setLoopInterval(int loopInterval)
        {
            m_loopInterval = loopInterval;
        }

        public int getLoopTimes()
        {
            return m_loopTimes;
        }

        public void setLoopTimes(int loopTimes)
        {
            m_loopTimes = loopTimes;
        }

        public void setPushID(int pushid)
        {
            m_pushID = pushid;
        }
        public int getPushID()
        {
            return m_pushID;
        }

        /// <summery> 设置角标下发的逻辑
        /// <list type="int">
        /// <item>
        /// <term>-1</term>
        /// <description>表示角标不变;</description>
        /// </item>
        /// <item>
        /// <term>-2</term>
        /// <description>表示角标自动加1;</description>
        /// </item>
        /// <item>
        /// <term>n</term>
        /// <description>(n>=0)表示自定义下发角标数字.</description>
        /// </item>
        /// </list>
        /// </summery>
        public void setBadgeType(int type) 
        {
            m_badgeType = type;
        }

        public int badgeType() 
        {
            return m_badgeType;
        }

        public Boolean isValid()
        {
            if (m_raw.Length != 0)
                return true;
            if (m_expireTime < 0 || m_expireTime > 3 * 24 * 60 * 60)
                return false;
            if ( m_type != (XGPushConstants.OrdinaryMessage) && m_type != (XGPushConstants.SilentMessage) && m_type != "0")
                return false;
            foreach (TimeInterval ti in m_acceptTimes)
            {
                if (!ti.isValid()) return false;
            }
            if (m_loopInterval > 0 && m_loopTimes > 0 && ((m_loopTimes - 1) * m_loopInterval + 1) > 15)
            {
                return false;
            }
            return true;
        }

        public object toJson()
        {
            if (m_raw.Length != 0)
                return m_raw;
            Dictionary<string,object> dict = new Dictionary<string, object>();
            dict.Add("accept_time",acceptTimeToJsonArray());
            Dictionary<string, object> aps = new Dictionary<string, object>();
            Dictionary<string, object> iOS = new Dictionary<string, object>();

            if(m_type.Equals(XGPushConstants.OrdinaryMessage) || m_type == "0")
            {
                Dictionary<string, object> alert = new Dictionary<string, object>();
                aps.Add("alert",alert);
                // aps.Add("badge",m_badge);
                aps.Add("badge_type", m_badgeType);
                if(m_sound.Length != 0)
                {
                    aps.Add("sound",m_sound);
                }
                if(m_category.Length != 0)
                {
                    aps.Add("category",m_category);
                }
                if (this.m_subtitle.Length != 0)
                {
                    iOS.Add("subtitle", this.m_subtitle);
                }
                if (this.m_title.Length != 0)
                {
                    dict.Add("title", this.m_title);
                }
                if (this.m_alertStr.Length != 0)
                {
                    dict.Add("content", m_alertStr);
                }

            } else if (m_type.Equals(XGPushConstants.SilentMessage)) {
                aps.Add("content-available", 1);
            }

            iOS.Add("aps",aps);
            if (this.m_custom != null)
            {
                foreach(var kvp in m_custom)
                {
                    iOS.Add(kvp.Key, kvp.Value);
                }
            }
            

            dict.Add("ios", iOS);
            
            return dict;
        }
    }

    
}
