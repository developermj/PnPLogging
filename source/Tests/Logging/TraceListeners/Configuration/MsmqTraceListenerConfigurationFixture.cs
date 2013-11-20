﻿// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.TestSupport.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Microsoft.Practices.EnterpriseLibrary.Logging.TestSupport;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Logging.Tests.TraceListeners.Configuration
{
    [TestClass]
    public class MsmqTraceListenerConfigurationFixture
    {
        TimeSpan timeSpan = new TimeSpan(10000);
        const MessagePriority priority = MessagePriority.VeryHigh;
        const string formatterName = "BinnaryFormatter";

        [TestInitialize]
        public void SetUp()
        {
            AppDomain.CurrentDomain.SetData("APPBASE", Environment.CurrentDirectory);
            ConfigurationTestHelper.ConfigurationFileName = "test.exe.config";
        }

        private static TraceListener GetListener(string name, IConfigurationSource configurationSource)
        {
            var settings = LoggingSettings.GetLoggingSettings(configurationSource);
            return settings.TraceListeners.Get(name).BuildTraceListener(settings);
        }

        [TestMethod]
        public void ListenerDataIsCreatedCorrectlyWithDefaults()
        {
            MsmqTraceListenerData listenerData =
                new MsmqTraceListenerData("listener", CommonUtil.MessageQueuePath, formatterName);

            Assert.AreSame(typeof(MsmqTraceListener), listenerData.Type);
            Assert.AreSame(typeof(MsmqTraceListenerData), listenerData.ListenerDataType);
            Assert.AreEqual("listener", listenerData.Name);
            Assert.AreEqual(formatterName, listenerData.Formatter);
            Assert.AreEqual(CommonUtil.MessageQueuePath, listenerData.QueuePath);
            Assert.AreEqual(MsmqTraceListenerData.DefaultRecoverable, listenerData.Recoverable);
            Assert.AreEqual(MsmqTraceListenerData.DefaultUseAuthentication, listenerData.UseAuthentication);
            Assert.AreEqual(MsmqTraceListenerData.DefaultUseDeadLetter, listenerData.UseDeadLetterQueue);
            Assert.AreEqual(MsmqTraceListenerData.DefaultUseEncryption, listenerData.UseEncryption);
            Assert.AreEqual(MsmqTraceListenerData.DefaultPriority, listenerData.MessagePriority);
            Assert.AreEqual(MsmqTraceListenerData.DefaultTimeToBeReceived, listenerData.TimeToBeReceived);
            Assert.AreEqual(MsmqTraceListenerData.DefaultTimeToBeReceived, listenerData.TimeToBeReceived);
            Assert.AreEqual(MsmqTraceListenerData.DefaultTransactionType, listenerData.TransactionType);
            Assert.AreEqual(TraceOptions.None, listenerData.TraceOutputOptions);
        }

        [TestMethod]
        public void ListenerDataIsCreatedCorrectly()
        {
            MsmqTraceListenerData listenerData =
                new MsmqTraceListenerData("listener", CommonUtil.MessageQueuePath, formatterName, priority,
                                          false, timeSpan, timeSpan, false, false, false,
                                          MessageQueueTransactionType.Single);

            Assert.AreSame(typeof(MsmqTraceListener), listenerData.Type);
            Assert.AreSame(typeof(MsmqTraceListenerData), listenerData.ListenerDataType);
            Assert.AreEqual("listener", listenerData.Name);
            Assert.AreEqual(formatterName, listenerData.Formatter);
            Assert.AreEqual(CommonUtil.MessageQueuePath, listenerData.QueuePath);
            Assert.AreEqual(false, listenerData.Recoverable);
            Assert.AreEqual(false, listenerData.UseAuthentication);
            Assert.AreEqual(false, listenerData.UseDeadLetterQueue);
            Assert.AreEqual(false, listenerData.UseEncryption);
            Assert.AreEqual(priority, listenerData.MessagePriority);
            Assert.AreEqual(timeSpan, listenerData.TimeToBeReceived);
            Assert.AreEqual(timeSpan, listenerData.TimeToBeReceived);
            Assert.AreEqual(MessageQueueTransactionType.Single, listenerData.TransactionType);
            Assert.AreEqual(TraceOptions.None, listenerData.TraceOutputOptions);
        }

        [TestMethod]
        public void CanDeserializeSerializedConfiguration()
        {
            string name = "name";
            bool recoverable = true;
            bool authentication = false;
            bool deadLetter = true;
            bool encryption = false;
            MessageQueueTransactionType transactionType = MessageQueueTransactionType.Automatic;

            TraceListenerData data = new MsmqTraceListenerData(name, CommonUtil.MessageQueuePath, formatterName, priority,
                                                               recoverable, timeSpan, timeSpan, authentication, deadLetter,
                                                               encryption, transactionType, TraceOptions.Callstack, SourceLevels.Critical);

            LoggingSettings settings = new LoggingSettings();
            settings.TraceListeners.Add(data);

            IDictionary<string, ConfigurationSection> sections = new Dictionary<string, ConfigurationSection>();
            sections[LoggingSettings.SectionName] = settings;
            IConfigurationSource configurationSource
                = ConfigurationTestHelper.SaveSectionsInFileAndReturnConfigurationSource(sections);

            LoggingSettings roSettigs = (LoggingSettings)configurationSource.GetSection(LoggingSettings.SectionName);

            Assert.AreEqual(1, roSettigs.TraceListeners.Count);
            Assert.IsNotNull(roSettigs.TraceListeners.Get(name));
            Assert.AreEqual(TraceOptions.Callstack, roSettigs.TraceListeners.Get(name).TraceOutputOptions);
            Assert.AreEqual(SourceLevels.Critical, roSettigs.TraceListeners.Get(name).Filter);
            Assert.AreSame(typeof(MsmqTraceListenerData), roSettigs.TraceListeners.Get(name).GetType());
            Assert.AreSame(typeof(MsmqTraceListenerData), roSettigs.TraceListeners.Get(name).ListenerDataType);
            Assert.AreSame(typeof(MsmqTraceListener), roSettigs.TraceListeners.Get(name).Type);
            Assert.AreEqual(formatterName, ((MsmqTraceListenerData)roSettigs.TraceListeners.Get(name)).Formatter);
            Assert.AreEqual(CommonUtil.MessageQueuePath, ((MsmqTraceListenerData)roSettigs.TraceListeners.Get(name)).QueuePath);
            Assert.AreEqual(recoverable, ((MsmqTraceListenerData)roSettigs.TraceListeners.Get(name)).Recoverable);
            Assert.AreEqual(authentication, ((MsmqTraceListenerData)roSettigs.TraceListeners.Get(name)).UseAuthentication);
            Assert.AreEqual(deadLetter, ((MsmqTraceListenerData)roSettigs.TraceListeners.Get(name)).UseDeadLetterQueue);
            Assert.AreEqual(encryption, ((MsmqTraceListenerData)roSettigs.TraceListeners.Get(name)).UseEncryption);
            Assert.AreEqual(priority, ((MsmqTraceListenerData)roSettigs.TraceListeners.Get(name)).MessagePriority);
            Assert.AreEqual(timeSpan, ((MsmqTraceListenerData)roSettigs.TraceListeners.Get(name)).TimeToBeReceived);
            Assert.AreEqual(timeSpan, ((MsmqTraceListenerData)roSettigs.TraceListeners.Get(name)).TimeToBeReceived);
            Assert.AreEqual(transactionType, ((MsmqTraceListenerData)roSettigs.TraceListeners.Get(name)).TransactionType);
        }

        [TestMethod]
        public void CanDeserializeSerializedConfigurationWithDefaults()
        {
            LoggingSettings rwLoggingSettings = new LoggingSettings();
            rwLoggingSettings.TraceListeners.Add(new MsmqTraceListenerData("listener", CommonUtil.MessageQueuePath, formatterName));

            IDictionary<string, ConfigurationSection> sections = new Dictionary<string, ConfigurationSection>();
            sections[LoggingSettings.SectionName] = rwLoggingSettings;
            IConfigurationSource configurationSource
                = ConfigurationTestHelper.SaveSectionsInFileAndReturnConfigurationSource(sections);

            LoggingSettings roLoggingSettings = (LoggingSettings)configurationSource.GetSection(LoggingSettings.SectionName);

            Assert.AreEqual(1, roLoggingSettings.TraceListeners.Count);
            Assert.IsNotNull(roLoggingSettings.TraceListeners.Get("listener"));
            Assert.AreEqual(roLoggingSettings.TraceListeners.Get("listener").GetType(), typeof(MsmqTraceListenerData));

            MsmqTraceListenerData listenerData = roLoggingSettings.TraceListeners.Get("listener") as MsmqTraceListenerData;
            Assert.AreEqual("listener", listenerData.Name);
            Assert.AreEqual(formatterName, listenerData.Formatter);
            Assert.AreEqual(CommonUtil.MessageQueuePath, listenerData.QueuePath);
            Assert.AreEqual(MsmqTraceListenerData.DefaultRecoverable, listenerData.Recoverable);
            Assert.AreEqual(MsmqTraceListenerData.DefaultUseAuthentication, listenerData.UseAuthentication);
            Assert.AreEqual(MsmqTraceListenerData.DefaultUseDeadLetter, listenerData.UseDeadLetterQueue);
            Assert.AreEqual(MsmqTraceListenerData.DefaultUseEncryption, listenerData.UseEncryption);
            Assert.AreEqual(MsmqTraceListenerData.DefaultPriority, listenerData.MessagePriority);
            Assert.AreEqual(MsmqTraceListenerData.DefaultTimeToBeReceived, listenerData.TimeToBeReceived);
            Assert.AreEqual(MsmqTraceListenerData.DefaultTimeToBeReceived, listenerData.TimeToBeReceived);
            Assert.AreEqual(TraceOptions.None, listenerData.TraceOutputOptions);
        }

        [TestMethod]
        public void CanDeserializeSerializedConfigurationWithOnlyRequiredProperties()
        {
            LoggingSettings rwLoggingSettings = new LoggingSettings();
            MsmqTraceListenerData data = new MsmqTraceListenerData();
            data.Name = "listener";
            data.QueuePath = CommonUtil.MessageQueuePath;
            data.Formatter = formatterName;
            data.Type = typeof(MsmqTraceListener);
            data.ListenerDataType = typeof(MsmqTraceListenerData);
            rwLoggingSettings.TraceListeners.Add(data);

            IDictionary<string, ConfigurationSection> sections = new Dictionary<string, ConfigurationSection>();
            sections[LoggingSettings.SectionName] = rwLoggingSettings;
            IConfigurationSource configurationSource
                = ConfigurationTestHelper.SaveSectionsInFileAndReturnConfigurationSource(sections);

            LoggingSettings roLoggingSettings = (LoggingSettings)configurationSource.GetSection(LoggingSettings.SectionName);

            Assert.AreEqual(1, roLoggingSettings.TraceListeners.Count);
            Assert.IsNotNull(roLoggingSettings.TraceListeners.Get("listener"));
            Assert.AreEqual(roLoggingSettings.TraceListeners.Get("listener").GetType(), typeof(MsmqTraceListenerData));

            MsmqTraceListenerData listenerData = roLoggingSettings.TraceListeners.Get("listener") as MsmqTraceListenerData;
            Assert.AreEqual("listener", listenerData.Name);
            Assert.AreEqual(formatterName, listenerData.Formatter);
            Assert.AreEqual(CommonUtil.MessageQueuePath, listenerData.QueuePath);
            Assert.AreEqual(MsmqTraceListenerData.DefaultRecoverable, listenerData.Recoverable);
            Assert.AreEqual(MsmqTraceListenerData.DefaultUseAuthentication, listenerData.UseAuthentication);
            Assert.AreEqual(MsmqTraceListenerData.DefaultUseDeadLetter, listenerData.UseDeadLetterQueue);
            Assert.AreEqual(MsmqTraceListenerData.DefaultUseEncryption, listenerData.UseEncryption);
            Assert.AreEqual(MsmqTraceListenerData.DefaultPriority, listenerData.MessagePriority);
            Assert.AreEqual(MsmqTraceListenerData.DefaultTimeToBeReceived, listenerData.TimeToBeReceived);
            Assert.AreEqual(MsmqTraceListenerData.DefaultTimeToBeReceived, listenerData.TimeToBeReceived);
            Assert.AreEqual(TraceOptions.None, listenerData.TraceOutputOptions);
        }

        [TestMethod]
        public void CanCreateInstanceFromGivenName()
        {
            TraceListenerData listenerData = new MsmqTraceListenerData("listener", CommonUtil.MessageQueuePath, formatterName);

            MockLogObjectsHelper helper = new MockLogObjectsHelper();
            helper.loggingSettings.TraceListeners.Add(listenerData);
            helper.loggingSettings.Formatters.Add(new BinaryLogFormatterData(formatterName));

            TraceListener listener = GetListener("listener", helper.configurationSource);

            Assert.IsNotNull(listener);
            Assert.AreEqual(listener.GetType(), typeof(MsmqTraceListener));

            MsmqTraceListener msmqTraceListener = listener as MsmqTraceListener;
            Assert.AreEqual("listener", listener.Name);
            Assert.IsNotNull(msmqTraceListener.Formatter);
            Assert.AreEqual(msmqTraceListener.Formatter.GetType(), typeof(BinaryLogFormatter));
            Assert.AreEqual(CommonUtil.MessageQueuePath, msmqTraceListener.QueuePath);
        }

        [TestMethod]
        public void CanCreateInstanceFromConfigurationFile()
        {
            LoggingSettings loggingSettings = new LoggingSettings();
            loggingSettings.Formatters.Add(new FormatterData(formatterName, typeof(BinaryLogFormatter)));
            loggingSettings.TraceListeners.Add(new MsmqTraceListenerData("listener", CommonUtil.MessageQueuePath, formatterName));

            TraceListener listener =
                GetListener("listener", CommonUtil.SaveSectionsAndGetConfigurationSource(loggingSettings));

            Assert.IsNotNull(listener);
            Assert.AreEqual(listener.GetType(), typeof(MsmqTraceListener));

            MsmqTraceListener msmqTraceListener = listener as MsmqTraceListener;
            Assert.IsNotNull(msmqTraceListener.Formatter);
            Assert.AreEqual(msmqTraceListener.Formatter.GetType(), typeof(BinaryLogFormatter));
            Assert.AreEqual(CommonUtil.MessageQueuePath, msmqTraceListener.QueuePath);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreatingInstanceWithNullFormatterThrows()
        {
            var settings = new LoggingSettings() { Formatters = { new BinaryLogFormatterData("formatter") } };
            new MsmqTraceListenerData() { QueuePath = "queue", Formatter = null }.BuildTraceListener(settings);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreatingInstanceWithEmptyFormatterThrows()
        {
            var settings = new LoggingSettings() { Formatters = { new BinaryLogFormatterData("formatter") } };
            new MsmqTraceListenerData() { QueuePath = "queue", Formatter = "" }.BuildTraceListener(settings);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreatingInstanceWithNullQueuePathThrows()
        {
            var settings = new LoggingSettings() { Formatters = { new BinaryLogFormatterData("formatter") } };
            new MsmqTraceListenerData() { QueuePath = null, Formatter = "formatter" }.BuildTraceListener(settings);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreatingInstanceWithEmptyQueuePathThrows()
        {
            var settings = new LoggingSettings() { Formatters = { new BinaryLogFormatterData("formatter") } };
            new MsmqTraceListenerData() { QueuePath = "", Formatter = "formatter" }.BuildTraceListener(settings);
        }

        [TestMethod]
        public void CannotCreateInstanceFromConfigurationFileInPartialTrustIfLoggingIsFullyTrusted()
        {
            LoggingSettings loggingSettings = new LoggingSettings();
            loggingSettings.Formatters.Add(new FormatterData(formatterName, typeof(BinaryLogFormatter)));
            loggingSettings.TraceListeners.Add(new MsmqTraceListenerData("listener", CommonUtil.MessageQueuePath, formatterName));
            var source = CommonUtil.SaveSectionsAndGetConfigurationSource(loggingSettings);

            var commonAssembly = typeof(Guard).Assembly.GetName();
            var loggingAssembly = typeof(LogWriter).Assembly.GetName();

            var fullyTrustedAssemblies = new[] { 
                    new StrongName(new StrongNamePublicKeyBlob(commonAssembly.GetPublicKey()), commonAssembly.Name, commonAssembly.Version),
                    new StrongName(new StrongNamePublicKeyBlob(loggingAssembly.GetPublicKey()), loggingAssembly.Name, loggingAssembly.Version)
                };
            var unsignedAssemblies = fullyTrustedAssemblies.Where(sn => sn.PublicKey.ToString() == "");
            if (unsignedAssemblies.Any())
            {
                Assert.Inconclusive("Full trust assemblies must be signed. This test will be ignored. Unsigned assemblies: " + unsignedAssemblies.Aggregate("", (a, sn) => a + sn.Name + " "));
            }

            var evidence = new Evidence();
            evidence.AddHostEvidence(new Zone(SecurityZone.Intranet));
            var set = SecurityManager.GetStandardSandbox(evidence);
            set.AddPermission(new SecurityPermission(SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.SerializationFormatter));
            set.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
            set.AddPermission(new FileIOPermission(PermissionState.Unrestricted));
            var appDomain =
                AppDomain.CreateDomain(
                    "partial trust",
                    null,
                    AppDomain.CurrentDomain.SetupInformation,
                    set,
                    fullyTrustedAssemblies);

            try
            {
                appDomain.DoCallBack(LoadConfiguration);

                Assert.Fail("Should have thrown");
            }
            catch (ConfigurationErrorsException)
            {
            }
            finally
            {
                AppDomain.Unload(appDomain);
            }
        }

        private static void LoadConfiguration()
        {
            var configurationSource = new FileConfigurationSource(ConfigurationTestHelper.ConfigurationFileName, false);
            configurationSource.GetSection(LoggingSettings.SectionName);
        }
    }
}
