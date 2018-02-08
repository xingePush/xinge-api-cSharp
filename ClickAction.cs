using System;
using System.Collections.Generic;


namespace XingeApp
{
    public class ClickAction
    {
        public static int TYPE_ACTIVITY = 1;
        public static int TYPE_URL = 2;
        public static int TYPE_INTENT = 3;

        private int m_actionType;
        private string m_url;
        private int m_confirmUrl;
        private string m_activity;
        private string m_intent;

        public void setActionType(int actionType) { this.m_actionType = actionType; }
        public void setActivity(string activity) { this.m_activity = activity; }
        public void setUrl(string url) { this.m_url = url; }
        public void setConfirmUrl(int confirmUrl) { this.m_confirmUrl = confirmUrl; }
        public void setIntent(string intent) { this.m_intent = intent; }

        public Dictionary<string,object> toJson()
        {
           Dictionary<string,object> dict = new Dictionary<string, object>();
            dict.Add("action_type", m_actionType);
            dict.Add("activity", m_activity);
            dict.Add("intent", m_intent);
            Dictionary<string, object> browser = new Dictionary<string, object>();
            browser.Add("url", m_url);
            browser.Add("confirm", m_confirmUrl);
            dict.Add("browser", browser);

            //string jsonData = JsonConvert.SerializeObject(dict);
            return dict;
        }


        public Boolean isValid()
        {
            if (m_actionType < TYPE_ACTIVITY || m_actionType > TYPE_INTENT)
                return false;
            if(m_actionType == TYPE_URL)
            {
                if (m_url.Length == 0 || m_confirmUrl < 0 || m_confirmUrl > 1)
                    return false;
                return true;
            }
            if(m_actionType == TYPE_INTENT)
            {
                if (m_intent.Length == 0)
                    return false;
                return true;
            }
            return true;
        }

        public ClickAction()
        {
            m_actionType = 1;
            m_activity = "";
            m_url = "";
            m_intent = "";
        }
    }
}
