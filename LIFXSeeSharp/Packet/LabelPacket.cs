﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LIFXSeeSharp.Bulb;
using System.Diagnostics;
using LIFXSeeSharp.Logging;

namespace LIFXSeeSharp.Packet
{
    class LabelPacket : BasePacket
    {
        private readonly string TAG = "LabelPacket";

        public override byte Type { get { return 0x0019; } }
        public string Label { get; private set; }

        public LabelPacket() : base()
        {
        }

        public LabelPacket(byte[] payload, IPAddress ip) : base(payload, ip)
        {
        }

        protected override void ProcessPayload()
        {
            base.ProcessPayload();

            var labelSubArray = new byte[32];
            Array.Copy(Payload, 36, labelSubArray, 0, 32);
            Label = Encoding.UTF8.GetString(labelSubArray).TrimEnd('\0');
        }

        public override void ProcessBulb(IBulb bulb)
        {
            Log.Debug(TAG, "Processing bulb label");
            var b = bulb as LifxBulb;
            if (b != null)
            {
                b.Label = Label;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            return sb.AppendFormat("Label packet: IP {0}, Label {1}, Sequence {2}", IP.ToString(), Label, Sequence).ToString();
        }
    }
}
