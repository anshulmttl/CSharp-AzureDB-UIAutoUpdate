using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectAzureDB
{
    class OrderItems
    {
        public String orderId;

        public String displayOutput;

        public String printOutput;
    }

    class SortedItems
    {
        
        // Maintain ordered dictionary
        public SortedDictionary<String, OrderItems> m_Dict = new SortedDictionary<String,OrderItems>();

        public String m_CurrentOrderId = String.Empty;

        public bool OrderedInsert(String key, OrderItems item)
        {
            bool newOrder = false;

            if (m_Dict.ContainsKey(key))
                return newOrder;

            newOrder = true;
            m_Dict.Add(key, item);

            return newOrder;
        }

        public void SetNextElement()
        {
            String index = m_CurrentOrderId;

            foreach(var pair in m_Dict.Reverse())
            {
                if (pair.Key == m_CurrentOrderId)
                    break;

                index = pair.Key;
            }

            m_CurrentOrderId = index;
        }

        public void SetBackElement()
        {
            string index = m_CurrentOrderId;

            foreach (var pair in m_Dict)
            {
                if (pair.Key == m_CurrentOrderId)
                    break;

                index = pair.Key;
            }

            m_CurrentOrderId = index;
        }

        public void Initialize()
        {
            if (m_Dict.Keys.Count <= 0)
                return;
            
            if (m_CurrentOrderId == String.Empty)
            {
                var first = m_Dict.First();
                m_CurrentOrderId = first.Key;
            }
        }

        public String GetDisplayText()
        {   
            if (m_CurrentOrderId == String.Empty)
                return String.Empty;

            return m_Dict[m_CurrentOrderId].displayOutput;
        }

        public String GetPrintText()
        {
            if (m_CurrentOrderId == String.Empty)
                return String.Empty;

            return m_Dict[m_CurrentOrderId].printOutput;
        }

        public bool GetNextPageStatus()
        {
            bool status = true;

            if (m_CurrentOrderId == String.Empty)
                return false;

            foreach(KeyValuePair<String,OrderItems> pair in m_Dict)
            {
                if (pair.Key == m_CurrentOrderId)
                    status = false;

                status = true; // Set status = true after CurrentOrderId
            }

            return status;
        }

        public bool GetGoBackStatus()
        {
            bool status = false;

            if (m_CurrentOrderId == String.Empty)
                return false;

            foreach(KeyValuePair<String,OrderItems> pair in m_Dict)
            {
                if (pair.Key == m_CurrentOrderId)
                    break;

                status = true; // If current OrderId is first status is false

            }

            return status;
        }
    }
}
