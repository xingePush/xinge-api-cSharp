using System;
using System.Collections.Generic;


namespace XingeApp
{
    public class TimeInterval
    {
        private int m_startHour;
        private int m_startMin;
        private int m_endHour;
        private int m_endMin;

        public TimeInterval(int startHour, int startMin, int endHour, int endMin)
        {
            this.m_startHour = startHour;
            this.m_startMin = startMin;
            this.m_endHour = endHour;
            this.m_endMin = endMin;
        }

        public Boolean isValid()
        {
            if (this.m_startHour >= 0 && this.m_startHour <= 23 &&
            this.m_startMin >= 0 && this.m_startMin <= 59 &&
            this.m_endHour >= 0 && this.m_endHour <= 23 &&
            this.m_endMin >= 0 && this.m_endMin <= 59)
                return true;
            else
                return false;
        }

        public Dictionary<string,object> toJson()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            Dictionary<string, object> start = new Dictionary<string, object>();
            Dictionary<string, object> end = new Dictionary<string, object>();
            start.Add("hour",m_startHour.ToString());
            start.Add("min",m_startMin.ToString());
            end.Add("hour",m_endHour.ToString());
            end.Add("min",m_endMin.ToString());
            dict.Add("start",start);
            dict.Add("end",end);

            return dict;
        }
    }
}
