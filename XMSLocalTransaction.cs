/****************************************************************************************/
/*                                                                                      */
/*                                                                                      */
/*  (c) Copyright IBM Corporation 2020                                                  */
/*                                                                                      */
/*  Licensed under the Apache License, Version 2.0 (the "License");                     */
/*  you may not use this file except in compliance with the License.                    */
/*  You may obtain a copy of the License at                                             */
/*                                                                                      */
/*  http://www.apache.org/licenses/LICENSE-2.0                                          */
/*                                                                                      */
/*  Unless required by applicable law or agreed to in writing, software                 */
/*  distributed under the License is distributed on an "AS IS" BASIS,                   */
/*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.            */
/*  See the License for the specific language governing permissions and                 */
/*  limitations under the License.                                                      */
/*                                                                                      */
/*                                                                                      */
/****************************************************************************************/
/*                                                                                      */
/*  FILE NAME:      XMSLocalTransaction.cs                                              */
/*                                                                                      */
/*  DESCRIPTION:    A XMS .Net Put application which is connecting to                   */
/*  1 Queue - 1 Queue Manager using single thread and 10 threads. This scenario         */
/*  covers messages put under sync point. A commit being issued after every             */
/*  100 messages. Each thread is putting 10k messages each on the same queue            */
/*  of a queue manager                                                                  */
/*                                                                                      */
/*                                                                                      */
/* How to Run:                                                                          */
/*  Usage:  XMSLocalTransaction -q queueName [-h host -p port -l channel                */
/*  -n numberOfMsgs -t numberOfThreads -msgType persistence -msgSize sizeofthemessage   */
/*  -shareCnv sharingconversations]                                                     */
/*                                                                                      */
/*  Ex:  XMSLocalTransaction -q Q1 -qm Test1 -h 9.149.133.32 -p 1414                    */
/*  -l SYSTEM.DEF.SVRCONN -n 10000 -t 10 -msgType 0 -msgSize 256 -shareCnv false        */
/*                                                                                      */
/* Parameters:                                                                          */
/* -qm ==> QueueManager                                                                 */
/* -q ==> QueueName                                                                     */
/* -h ==> remotehost where queuemanager is running                                      */
/* -p ==> Port on which listener is running                                             */
/* -l ==> Channel                                                                       */
/* -msgType ==> Persistence or Non-Persistence                                          */
/* -n ==> No of messages                                                                */
/* -t ==> No of Threads                                                                 */
/* -msgSize ==> Size of the message in bytes                                            */
/* -shareCnv ==> Sharing Conversations                                                  */
/*                                                                                      */
/****************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBM.XMS;
using System.Diagnostics;

namespace XMSLocalTransaction
{
    class XMSLocalTransaction
    {      
        /// <summary>
        /// Dictionary to store the properties
        /// </summary>
        private IDictionary<string, object> properties = null;
        /// <summary>
        /// Expected command-line arguments
        /// </summary>
        private String[] cmdArgs = { "-q", "-h", "-p", "-l", "-qm", "-n", "-msgType", "-msgSize", "-shareCnv", "-t" };
        /// <summary>
        /// Array to note down the time taken by each thread
        /// </summary>
        List<double> myarray = new List<double>();
                
        private IConnection connectionWMQ = null;
        

        static void Main(string[] args)
        {
            try
            {
                XMSLocalTransaction p = new XMSLocalTransaction
                {
                    properties = new Dictionary<string, object>()
                };

                if (p.ParseCommandline(args))
                    p.CreateThreadOperations();                
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Invalid arguments!\n{0}", ex);
            }
            catch (XMSException ex)
            {
                Console.WriteLine("XMSException caught: {0}", ex);
                if (ex.LinkedException != null)
                {
                    Console.WriteLine("Stack Trace:\n {0}", ex.LinkedException.StackTrace);
                }
                Console.WriteLine("Sample execution  FAILED!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: {0}", ex);
                Console.WriteLine("Sample execution  FAILED!");
            }
        }

        /// <summary>
        /// Thread Creation
        /// </summary>
        public void CreateThreadOperations()
        {
            int numberOfThreads = Convert.ToInt32(properties["NoofThreads"]);
            var numberOfMsgs = Convert.ToInt32(properties["NoofMessages"]);
            var msgSize = Convert.ToInt32(properties["MessageSize"]);

            Task[] task = new Task[numberOfThreads];
            connectionWMQ = CreateConnection();
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("PERFORMANCE STATISTICS");
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("Creating Threads:                         " + numberOfThreads);
            Console.WriteLine("Number of Messages:                       " + numberOfMsgs);
            Console.WriteLine("Message Size:                             " + msgSize);
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("Waiting to get statistics... ");
            for (int i = 0; i < numberOfThreads; ++i)
            {
                task[i] = Task.Factory.StartNew(() => SendMessage());
            }
            Task.WaitAll(task);
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("Summary:");
            var max = myarray[0];
            for(int j=0; j < numberOfThreads; ++j)
            {
                if (myarray[j] > max)
                {
                    max = myarray[j];
                }                
            }
            Console.WriteLine("Transfer Rate = Total Number of Messages/ Maximum Time taken  = " + numberOfMsgs+ "/" +max+ " = "  + numberOfMsgs/max + " messages/second ");
            Console.WriteLine("-----------------------------------------------------");
        }

        /// <summary>
        /// Creating Queue Manager Connection
        /// </summary>
        /// <returns></returns>
        private IConnection CreateConnection()
        {
            // Get an instance of factory.
            var factoryFactory = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);

            // Create WMQ Connection Factory.
            var cf = factoryFactory.CreateConnectionFactory();

            // Set the properties
            cf.SetStringProperty(XMSC.WMQ_HOST_NAME, (String)properties[XMSC.WMQ_HOST_NAME]);
            cf.SetIntProperty(XMSC.WMQ_PORT, Convert.ToInt32(properties[XMSC.WMQ_PORT]));
            cf.SetStringProperty(XMSC.WMQ_CHANNEL, (String)properties[XMSC.WMQ_CHANNEL]);
            cf.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
            cf.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, (String)properties[XMSC.WMQ_QUEUE_MANAGER]);
            var shareCnv = Convert.ToBoolean(properties["ShareCnv"]);
            if (!shareCnv)
            {
                cf.SetIntProperty(XMSC.WMQ_SHARE_CONV_ALLOWED, XMSC.WMQ_SHARE_CONV_ALLOWED_NO);
            }

            return cf.CreateConnection();
        }

        /// <summary>
        /// Send message
        /// </summary>
        void SendMessage()
        {
            try
            {
                // Create session
                using (var sessionWMQ = connectionWMQ.CreateSession(true, AcknowledgeMode.AutoAcknowledge))
                {
                    // Create destination
                    var destination = sessionWMQ.CreateQueue((string)properties[XMSC.WMQ_QUEUE_NAME]);
                    // Create producer
                    var producer = sessionWMQ.CreateProducer(destination);

                    var msgType = Convert.ToInt32(properties["Persistence"]);
                    if (msgType == 1)
                        producer.DeliveryMode = DeliveryMode.Persistent;
                    else
                        producer.DeliveryMode = DeliveryMode.NonPersistent;
                    var msgSize = Convert.ToInt32(properties["MessageSize"]);
                    var str = new String('*', msgSize);
                    Byte[] msg = Encoding.UTF8.GetBytes(str);
                    var timer = new Stopwatch();

                    //Console.WriteLine("Sending Messages to queue to calculate the throughput");

                    var numberOfMsgs = Convert.ToInt32(properties["NoofMessages"]);
                    timer.Start();
                    IBytesMessage bytesMsg;
                    for (int i = 1; i <= numberOfMsgs; ++i)
                    {
                        // Create a text message and send it.
                        bytesMsg = sessionWMQ.CreateBytesMessage();
                        bytesMsg.WriteBytes(msg);

                        producer.Send(bytesMsg);
                        if (i % 100 == 0)
                            sessionWMQ.Commit();
                    }
                    timer.Stop();
                    Console.WriteLine("Time taken by " + Task.CurrentId + " is :" + timer.Elapsed.TotalSeconds);
                    myarray.Add(timer.Elapsed.TotalSeconds);

                    producer.Close();

                }
            }
            catch (XMSException ex)
            {
                Console.WriteLine("XMSException caught: {0}", ex);
                if (ex.LinkedException != null)
                {
                    Console.WriteLine("Stack Trace:\n {0}", ex.LinkedException.StackTrace);
                }
                Console.WriteLine("Sample execution  FAILED!");
            }
        }

        /// <summary>
        /// Parse commandline parameters        
        /// </summary>
        /// <param name="args"></param>
        bool ParseCommandline(string[] args)
        {
            try
            {
                if (args.Length < 4)
                {
                    DisplayHelp();
                    return false;
                }

                var cmdlineArguments = Enumerable.Range(0, args.Length / 2).ToDictionary(i => args[2 * i], i => args[2 * i + 1]);

                foreach (String arg in cmdlineArguments.Keys)
                {
                    if (!cmdArgs.Contains(arg))
                        throw new ArgumentException("Invalid argument", arg);
                }

                // set the properties
                properties.Add(XMSC.WMQ_HOST_NAME, cmdlineArguments.ContainsKey("-h") ? cmdlineArguments["-h"] : "localhost");
                properties.Add(XMSC.WMQ_PORT, cmdlineArguments.ContainsKey("-p") ? Convert.ToInt32(cmdlineArguments["-p"]) : 1414);
                properties.Add(XMSC.WMQ_CHANNEL, cmdlineArguments.ContainsKey("-l") ? cmdlineArguments["-l"] : "SYSTEM.DEF.SVRCONN");
                properties.Add(XMSC.WMQ_QUEUE_MANAGER, cmdlineArguments["-qm"]);
                properties.Add(XMSC.WMQ_QUEUE_NAME, cmdlineArguments["-q"]);
                properties.Add("ShareCnv", cmdlineArguments.ContainsKey("-shareCnv") ? Convert.ToBoolean(cmdlineArguments["-shareCnv"]) : false);
                properties.Add("MessageSize", cmdlineArguments.ContainsKey("-msgSize") ? Convert.ToInt32(cmdlineArguments["-msgSize"]) : 256);
                properties.Add("Persistence", cmdlineArguments.ContainsKey("-msgType") ? Convert.ToInt32(cmdlineArguments["-msgType"]) : 0);
                properties.Add("NoofThreads", cmdlineArguments.ContainsKey("-t") ? Convert.ToInt32(cmdlineArguments["-t"]) : 10);
                properties.Add("NoofMessages", cmdlineArguments.ContainsKey("-n") ? Convert.ToInt32(cmdlineArguments["-n"]) : 1000);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception caught while parsing command line arguments: " + e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Help Section
        /// </summary>
        void DisplayHelp()
        {
            Console.WriteLine("Usage: XMSCoreLocalTransaction -q queueName -qm queueManagerName [-h host -p port -l channel -n numberOfMsgs -t numberOfThreads -shareCnv sharingConversations -msgsize messageSize -msgType Persistence]");
            Console.WriteLine("- queueName    : a queue name");
            Console.WriteLine("- host         : hostname like 192.122.178.78. Default hostname is localhost");
            Console.WriteLine("- port         : port number like 3555. Default port is 1414");
            Console.WriteLine("- channel      : connection channel. Default is SYSTEM.DEF.SVRCONN");
            Console.WriteLine("- numberOfMsgs : The number of messages per thread. Default is 10000");
            Console.WriteLine("- numberOfThreads : The number of threads to be created. Default is 10");
            Console.WriteLine("- shareCnv     : Sharing conversations allowed . Default is true");
            Console.WriteLine("- messageSize : Size of the message. Default is 512");
            Console.WriteLine("- Persistence : Persistence or Non Persistence. Default is Non Persistence");            
            Console.WriteLine();
        }

    }
}
