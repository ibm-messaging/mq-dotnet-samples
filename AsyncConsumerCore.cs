/*****************************************************************************************/
/*                                                                                       */
/*                                                                                       */
/*  (c) Copyright IBM Corporation 2020                                                   */
/*                                                                                       */
/*  Licensed under the Apache License, Version 2.0 (the "License");                      */
/*  you may not use this file except in compliance with the License.                     */
/*  You may obtain a copy of the License at                                              */
/*                                                                                       */
/*  http://www.apache.org/licenses/LICENSE-2.0                                           */
/*                                                                                       */
/*  Unless required by applicable law or agreed to in writing, software                  */
/*  distributed under the License is distributed on an "AS IS" BASIS,                    */
/*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.             */
/*  See the License for the specific language governing permissions and                  */
/*  limitations under the License.                                                       */
/*                                                                                       */
/*                                                                                       */
/*****************************************************************************************/
/*                                                                                       */
/*                                                                                       */
/*                  IBM Message Service Client for .NET                                  */
/*                                                                                       */
/* FILE NAME:      AsyncConsumer.cs                                                      */
/* DESCRIPTION:    Basic example of simple asynchronous consumer.                        */
/*                                                                                       */
/* How to Run:                                                                           */
/* dotnet AsyncConsumerCore -qm queueManager -q queueName -h host -p port -l channel     */
/* dotnet AsyncConsumerCore -qm QM -q Q1 -h remotehost -p 1414 -l SYSTEM.DEF.SVRCONN     */
/*                                                                                       */
/* Parameters:                                                                           */
/* -qm ==> QueueManager                                                                  */
/* -q ==> QueueName                                                                      */
/* -h ==> remotehost where queuemanager is running                                       */
/* -p ==> Port on which listener is running                                              */
/* -l ==> Channel                                                                        */
/*                                                                                       */
/*                                                                                       */
/*****************************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IBM.XMS;

namespace SimpleAsyncConsumer
{
    class AsyncConsumer
    {

        /// <summary>
        /// Dictionary to store the properties
        /// </summary>
        private IDictionary<string, object> properties = null;
        /// <summary>
        /// Expected command-line arguments
        /// </summary>
        private String[] cmdArgs = { "-q", "-h", "-p", "-l", "-qm", "-n", "-msgType", "-msgSize", "-shareCnv", "-t" };

        ISession sessionWMQ =  null;

        public int numberOfMsgs = 1;

        public static Stopwatch timer;


        static void Main(string[] args)
        {
            Console.WriteLine("===> START of Simple AsyncConsumer Core sample. <===\n\n");
            try
            {
                AsyncConsumer p = new AsyncConsumer
                {
                    properties = new Dictionary<string, object>()
                };

                if (p.ParseCommandline(args))
                    p.ReceiveMessagesAsynchronous();
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
            Console.WriteLine("===> END of Asynchronous Consumer sample. <===\n\n");
        }

        void ReceiveMessagesAsynchronous()
        {
            try
            {
                XMSFactoryFactory factoryFactory;
                IConnectionFactory cf;
                IConnection connectionWMQ;
                
                factoryFactory = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);
                cf = factoryFactory.CreateConnectionFactory();

                cf.SetStringProperty(XMSC.WMQ_HOST_NAME, (String)properties[XMSC.WMQ_HOST_NAME]);
                cf.SetIntProperty(XMSC.WMQ_PORT, Convert.ToInt32(properties[XMSC.WMQ_PORT]));
                cf.SetStringProperty(XMSC.WMQ_CHANNEL, (String)properties[XMSC.WMQ_CHANNEL]);
                cf.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
                cf.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, (String)properties[XMSC.WMQ_QUEUE_MANAGER]);


                connectionWMQ = cf.CreateConnection();                
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine("PERFORMANCE STATISTICS");
                Console.WriteLine("-----------------------------------------------------");

                using (sessionWMQ = connectionWMQ.CreateSession(true, AcknowledgeMode.AutoAcknowledge))
                {
                    var destination = sessionWMQ.CreateQueue((string)properties[XMSC.WMQ_QUEUE_NAME]);

                    var consumerAsync = sessionWMQ.CreateConsumer(destination);

                    var messageListener = new MessageListener(OnMessageCallback);
                    consumerAsync.MessageListener = messageListener;                    
                    Console.WriteLine("Summary:");
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine("Registered Message Listener for 10k messages");
                    Console.WriteLine("Waiting to get statistics... ");
                    Console.WriteLine("-----------------------------------------------------");
                    timer = new Stopwatch();

                    connectionWMQ.Start();
                    
                    Console.ReadKey();                                                          
                    connectionWMQ.Stop();
                    
                    // Cleanup
                    consumerAsync.Close();
                    connectionWMQ.Close();
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

        bool complete = false;
        void OnMessageCallback(IMessage message)
        {
            try
            {                
                numberOfMsgs++;              
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in OnMessageCallback: {0}", ex);
                Environment.Exit(1);
            }
            if (numberOfMsgs == 500)
                timer.Start();
            if (numberOfMsgs == 9490)
            {
                sessionWMQ.Commit();
                timer.Stop();
                Console.WriteLine("Message Consumption Rate = Total Time taken to consume 10000 messages  = " + 10000/timer.Elapsed.TotalSeconds + " messages/second");
                Console.WriteLine("Press any key!");
                complete = true;
            }
            sessionWMQ.Commit();
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
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Invalid arguments!\n{0}", ex);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception caught while parsing command line arguments: " + e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }            
        }

        void DisplayHelp()
        {
            Console.WriteLine("Usage: dotnet AsyncConsumerCore.dll -q queueName -qm QM1 [-h host -p port -l channel]");
            Console.WriteLine("- queueName    : a queue name");
            Console.WriteLine("- queueManagerName : a queueManager name");
            Console.WriteLine("- host         : hostname like 192.122.178.78. Default hostname is localhost");
            Console.WriteLine("- port         : port number like 3555. Default port is 1414");
            Console.WriteLine("- channel      : connection channel. Default is SYSTEM.DEF.SVRCONN");            
            Console.WriteLine("    dotnet AsyncConsumerCore.dll -q B -qm QM1 -h remotehost -p 1414 -l SYSTEM.DEF.SVRCONN");
            Console.WriteLine();
        }

      
    }
}
