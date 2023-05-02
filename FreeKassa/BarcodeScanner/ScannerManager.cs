using System;
using System.IO.Ports;
using FreeKassa.Extensions.KassaManagerExceptions;
using FreeKassa.Utils;

namespace FreeKassa.BarcodeScanner
{
    public class ScannerManager : IDisposable
    {
        private readonly SerialPort _serial;
        private readonly SimpleLogger _logger;

        public delegate void ScannedCode(string code);

        public event ScannedCode Successfully;
        
        public ScannerManager(string serialPort, int baundRate, SimpleLogger logger)
        {
            _logger = logger;
            _serial = new SerialPort(serialPort, baundRate, Parity.None, 8, StopBits.One);
            _serial.DtrEnable = true;
            _serial.RtsEnable = true;
        }

        public void StartReading()
        {
            _serial.DataReceived += SerialOnDataReceived;
        }

        public void StopReading()
        {
            _serial.DataReceived -= SerialOnDataReceived;
        }

        private void SerialOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Successfully?.Invoke(_serial.ReadExisting());
        }

        public void Dispose()
        {
            _serial?.Dispose();
        }
    }
}