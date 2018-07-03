using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XingeApp
{
    public class Message
    {
        private string m_title;
        private string m_content;
        private int m_expireTime;
        private string m_sendTime;
        private List<TimeInterval> m_acceptTimes;
        private string m_type;
        private int m_multiPkg;
        private Style m_style;
        private ClickAction m_action;
        private Dictionary<string, object> m_custom;
        private string m_raw;
        private int m_loopInterval;
        private int m_loopTimes;

        public Message()
        {
            this.m_title = "";
            this.m_content = "";
            this.m_sendTime = "";
            this.m_acceptTimes = new List<TimeInterval>();
            this.m_multiPkg = 0;
            this.m_raw = "";
            this.m_loopInterval = -1;
            this.m_loopTimes = -1;
            this.m_action = new ClickAction();
            this.m_style = new Style(0);
        }

        public void setTitle(String title)
        {
            this.m_title = title;
        }
        public void setContent(String content)
        {
            this.m_content = content;
        }
        public void setExpireTime(int expireTime)
        {
            this.m_expireTime = expireTime;
        }
        public int getExpireTime()
        {
            return this.m_expireTime;
        }
        public void setSendTime(String sendTime)
        {
            this.m_sendTime = sendTime;
        }
        public String getSendTime()
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
        public void setType(string type)
        {
            this.m_type = type;
        }
        public string getType()
        {
            return m_type;
        }
        public void setMultiPkg(int multiPkg)
        {
            this.m_multiPkg = multiPkg;
        }
        public int getMultiPkg()
        {
            return m_multiPkg;
        }
        public void setStyle(Style style)
        {
            this.m_style = style;
        }
        public void setAction(ClickAction action)
        {
            this.m_action = action;
        }
        public void setCustom(Dictionary<String, Object> custom)
        {
            this.m_custom = custom;
        }
        public void setRaw(String raw)
        {
            this.m_raw = raw;
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
            if (m_type != (XGPushConstants.OrdinaryMessage) && m_type != (XGPushConstants.SilentMessage))
                return false;
            if (m_multiPkg < 0 || m_multiPkg > 1)
                return false;
            if (m_type == (XGPushConstants.OrdinaryMessage))
            {
                if (!m_style.isValid()) return false;
                if (!m_action.isValid()) return false;
            }
            if (m_expireTime < 0 || m_expireTime > 3 * 24 * 60 * 60)
                return false;
            foreach(TimeInterval ti in m_acceptTimes)
            {
                if (!ti.isValid()) return false;
            }
            if(m_loopInterval > 0 && m_loopTimes > 0 && ((m_loopTimes - 1) * m_loopInterval + 1) > 15)
            {
                return false;
            }
            return true;
        }

        public object toJson()
        {
            if (m_raw.Length != 0)
                return m_raw;
            Dictionary<string, object> dict = new Dictionary<string, object>();
            Dictionary<string, object> message = new Dictionary<string, object>();
            if (m_type.Equals(XGPushConstants.OrdinaryMessage))
            {
                dict.Add("title", m_title);
                dict.Add("content", m_content);
                dict.Add("accept_time", acceptTimeToJsonArray());
                dict.Add("builder_id", m_style.getBuilderId());
                dict.Add("ring", m_style.getRing());
                dict.Add("vibrate", m_style.getVibrate());
                dict.Add("clearable", m_style.getClearable());
                dict.Add("n_id", m_style.getNId());
                dict.Add("ring_raw", m_style.getRingRaw());
                dict.Add("lights", m_style.getLights());
                dict.Add("icon_type", m_style.getIconType());
                dict.Add("icon_res", m_style.getIconRes());
                dict.Add("style_id", m_style.getStyleId());
                dict.Add("small_icon", m_style.getSmallIcon());
                dict.Add("action", m_action.toJson());
            }
            else if(m_type.Equals(XGPushConstants.SilentMessage))
            {
                dict.Add("title", m_title);
                dict.Add("content", m_content);
                dict.Add("accept_time", acceptTimeToJsonArray());
            }
            
            if (this.m_custom != null)
            {
                foreach(var kvp in m_custom)
                {
                    dict.Add(kvp.Key, kvp.Value);
                }
            }

            return dict;
        }
    }

    
}
