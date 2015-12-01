﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LIFXSeeSharp
{
	public class LifxController
	{
		private static readonly int NUM_BULBS = 4;

		[DllImport("LIFX.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void Discover();

		[DllImport("LIFX.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		private static extern bool PopulateLabels([Out] IntPtr[] labels);

		[DllImport("LIFX.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		private static extern bool SetPower(string label, ushort onoff);

		[DllImport("LIFX.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		private static extern bool SetLightColor(string label, ushort[] state);

		[DllImport("LIFX.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		private static extern bool GetLightState(string label, [Out] IntPtr[] state);

		private List<LifxBulb> Bulbs { get; set; }

		public LifxController()
		{
			
		}

		public void RunInitialDiscovery()
		{
			Discover();

			Bulbs = new List<LifxBulb>();
			var labels = new IntPtr[NUM_BULBS];
			PopulateLabels(labels);
			var names = new string[labels.Length];

			for (var i = 0; i < labels.Length; ++i) {
				names[i] = Marshal.PtrToStringBSTR(labels[i]);
				Marshal.FreeBSTR(labels[i]);
				Bulbs.Add(new LifxBulb(names[i]));
			}
		}

		public void GetAllLightStates()
		{
			Bulbs.ForEach(b =>
			{
				var state = new IntPtr[6];
				GetLightState(b.Label, state);

				// Hue => 0,360
				// Saturation => %
				// Brightness => %
				// Kelvin => 2500,9000
				// Dim => ms
				// Power => 0 & 65535
				b.Hue = (float)state[0] * 360 / ushort.MaxValue;
				b.Saturation = (float)state[1] / ushort.MaxValue;
				b.Brightness = (float)state[2] / ushort.MaxValue;
				b.Kelvin = (ushort)state[3];
				b.Dim = (ushort)state[4];
				b.Power = (ushort)state[5];
			});
		}

		public void GetLightState(string label = null)
		{
			if (label != null)
			{
				Bulbs.Where(b => b.Label == label)
					.ToList()
					.ForEach(b => 
					{
						var state = new IntPtr[6];
						GetLightState(b.Label, state);

						// Hue => 0,360
						// Saturation => %
						// Brightness => %
						// Kelvin => 2500,9000
						// Dim => ms
						// Power => 0 & 65535
						b.Hue = (float)state[0] * 360 / ushort.MaxValue;
						b.Saturation = (float)state[1] / ushort.MaxValue;
						b.Brightness = (float)state[2] / ushort.MaxValue;
						b.Kelvin = (ushort)state[3];
						b.Dim = (ushort)state[4];
						b.Power = (ushort)state[5];
					});
			}
			else
			{
				Bulbs.ForEach(b =>
						{
							var state = new IntPtr[6];
							GetLightState(b.Label, state);

							// Hue => 0,360
							// Saturation => %
							// Brightness => %
							// Kelvin => 2500,9000
							// Dim => ms
							// Power => 0 & 65535
							b.Hue = (float)state[0] * 360 / ushort.MaxValue;
							b.Saturation = (float)state[1] / ushort.MaxValue;
							b.Brightness = (float)state[2] / ushort.MaxValue;
							b.Kelvin = (ushort)state[3];
							b.Dim = (ushort)state[4];
							b.Power = (ushort)state[5];
						});
			}
		}

		public void SetLightState(float hue, float saturation, float brightness, ushort kelvin, ushort dim, string target = null)
		{
			if (kelvin > 9000 || kelvin < 2500) {
				throw new ArgumentOutOfRangeException(nameof(kelvin),
													kelvin,
													"Kelvin should be betweeen 2500 & 9000");
			}

			var fhue = hue % 360;
			var fsaturation = saturation % 100;
			var fbrightness = brightness % 100;

			var state = new ushort[5];

			state[0] = Convert.ToUInt16(fhue * ushort.MaxValue / 360); // hue * max / 360
			state[1] = Convert.ToUInt16(fsaturation * ushort.MaxValue); // %
			state[2] = Convert.ToUInt16(fbrightness * ushort.MaxValue); // %
			state[3] = kelvin; // between 2500 & 9000
			state[4] = dim;

			if (target != null)
			{
				Bulbs.Where(b => b.Label == target).ToList().ForEach(b =>
				{
					SetLightColor(b.Label, state);
				});
			}
			else
			{
				Bulbs.ForEach(b =>
				{
					SetLightColor(b.Label, state);
				});	
			}
		}

		public void SetPower(ushort onoff, string target = null)
		{
			if (target != null)
			{
				Bulbs.Where(b => b.Label == target).ToList().ForEach(b =>
				{
					SetPower(b.Label, onoff);
				});
			}
			else
			{
				Bulbs.ForEach(b =>
				{
					SetPower(b.Label, onoff);
				});
			}
		}
	}
}