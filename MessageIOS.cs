using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XingeApp
{
    public class MessageIOS
    {
        public static int TYPE_APNS_NOTIFICATION = 0;
        //public static int TYPE_REMOTE_NOTIFICATION = 12;

        private int m_expireTime;
        private string m_sendTime;
        private List<TimeInterval> m_acceptTimes;
        private int m_type;
        private Dictionary<string, object> m_custom;
        private string m_raw;
        private string m_alertStr;
        private Dictionary<string,object> m_alertJo;
        private int m_badge;
        private string m_sound;
        private string m_category;
        private int m_loopInterval;
        private int m_loopTimes;

        public MessageIOS()
        {
            this.m_sendTime = "2014-03-13 16:13:00";
            this.m_acceptTimes = new List<TimeInterval>();
            this.m_raw = "";
            this.m_alertStr = "";
            this.m_badge = 0;
            this.m_sound = "";
            this.m_category = "";
            this.m_loopInterval = -1;
            this.m_loopTimes = -1;
            this.m_type = TYPE_APNS_NOTIFICATION;
        }

        public void setType(int type)
        {
            this.m_type = type;
        }

        public int getType()
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

        public void setBadge(int badge)
        {
            m_badge = badge;
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

        public Boolean isValid()
        {
            if (m_raw.Length != 0)
                return true;
            if (m_expireTime < 0 || m_expireTime > 3 * 24 * 60 * 60)
                return false;
            if (m_type != TYPE_APNS_NOTIFICATION )
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

        public string toJson()
        {
            if (m_raw.Length != 0)
                return m_raw;
            Dictionary<string,object> dict = new Dictionary<string, object>();
            dict.Add("accept_time",acceptTimeToJsonArray());
            Dictionary<string, object> aps = new Dictionary<string, object>();
            if(m_type == TYPE_APNS_NOTIFICATION)
            {
                aps.Add("alert",m_alertStr);
                if(m_badge != 0)
                {
                    aps.Add("badge",m_badge);
                }
                if(m_sound.Length != 0)
                {
                    aps.Add("sound",m_sound);
                }
                if(m_category.Length != 0)
                {
                    aps.Add("category",m_category);
                }
            }
            dict.Add("aps",aps);
            return JsonConvert.SerializeObject(dict);
        }
    }

    
}
