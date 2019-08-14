using MQTTnet.Client;
using MQTTnet.Client.Options;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIEU.Protocol.Mqtt
{
    public abstract class AbsPeiuMqttBase
    {
        // log4j object
        protected Logger logger = null;

        private bool initialized = false;

        //=====================================================
        // MQTT variables
        //=====================================================
        protected MqttClient mqttClient = null;
        protected MqttClientOptions mqttClientOptions = null;

        protected bool shutDown = false;

        protected String mqttClientId = null;
        protected String mqttBrokerUrl = null;
        protected String mqttCaCertPath = null;
        protected String mqttClientCertPath = null;
        protected String mqttClientKeyPath = null;
        protected String mqttClientKeyPassword = null;
        protected bool isMqttBrokerSsl = false;
        protected String mqttSslProtocol = null;
        protected String mqttUsername = null;
        protected String mqttPassword = null;
        protected int mqttReconnectionDelayInterval = -1;
        //protected XsdConverterFormat mqttMessageFormat = null;
        protected bool mqttIsFormattedOutput = false;
        protected bool mqttIsCleanSession = false;

        protected String propertiesFilePath = null;

        private void setPropertiesFilePath()
        {
            propertiesFilePath = System.Environment.GetEnvironmentVariable("OPENFMB_PROPERTIES_FILE_PATH");
            if (string.IsNullOrEmpty(propertiesFilePath))
            {
                propertiesFilePath = "";
            }

        }

        /**
	 * This method should be called first so the log4j logger can be created.
	 */
        protected void initializeLogger()
        {
            setPropertiesFilePath();

            // Initial the logger objects.
            logger = Logger.getLogger(this.getClass().getSimpleName());

            // Load the properties for the log4j loggers.

            PropertyConfigurator.configure(propertiesFilePath + this.getClass().getSimpleName() + ".log4j.properties");
        }
    }
}
