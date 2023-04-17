using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyModbus;

namespace AppPlcSQLiteLaser
{
    class ModbusTCPClient
    {
        /*
        //<Modbus Client>
        ModbusClient mPLCM340 = new ModbusClient();
        public void mConnect()
        {
            mPLCM340.IPAddress = "10.187.212.120";
            mPLCM340.Port = Convert.ToInt32(502);
            mPLCM340.ConnectionTimeout = Convert.ToInt32(5000);
            mPLCM340.Connect();
        }
        private Task<string> ReadRegAsync(int address, ModbusClient client)
        {
            return Task.Run(() => {
                return ModbusClient.ConvertRegistersToFloat(
                                          client.ReadHoldingRegisters(address, 2),
                                          ModbusClient.RegisterOrder.HighLow)
                                   .ToString();
            });
        }
        private Task WriteRegAsync(float variable, ModbusClient client)
        {
            return Task.Run(() => {
                client.WriteMultipleRegisters(
                             2,
                             ModbusClient.ConvertFloatToRegisters(variable,
                                                                   ModbusClient.RegisterOrder.HighLow)
                 );
            });
        }
        */
    }

}
