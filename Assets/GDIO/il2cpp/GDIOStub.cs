using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using static UnityEngine.InputSystem.LowLevel.InputEventTrace;
using Unity.Collections.LowLevel.Unsafe;
#endif
public class GDIOStub : MonoBehaviour
{
	[UnityEngine.Scripting.Preserve]
	public void GDIOStubCall()
	{
		AudioSource source = new AudioSource();
		bool sMute = source.mute;
		bool sPlaying = source.isPlaying;
		float sVolume = source.volume;

#if ENABLE_INPUT_SYSTEM
			gdio.unity_agent.XRSimulatedInput.GetCurrentFrame();
	        Type t = typeof(System.Action<UnityEngine.InputSystem.InputDevice, UnityEngine.InputSystem.LowLevel.KeyboardState, double>);
        	Type t2 = typeof(System.Action<UnityEngine.InputSystem.InputDevice, UnityEngine.InputSystem.LowLevel.MouseState, double>);
			Type t3 = typeof(InternedString);
			Type t4 = typeof(UnityEngine.InputSystem.InputSystem);
	        Type t5 = typeof(UnityEngine.InputSystem.InputDevice);

	        Type t8 = typeof(System.Action<System.Object, UnityEngine.InputSystem.LowLevel.KeyboardState, double>);
        	Type t9 = typeof(System.Action<System.Object, UnityEngine.InputSystem.LowLevel.MouseState, double>);

	        object a = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(t8);
        	object b = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(t9);

	        UnityEngine.InputSystem.InputSystem.QueueStateEvent<UnityEngine.InputSystem.LowLevel.KeyboardState>((InputDevice)null, (KeyboardState)new KeyboardState(), -1d);
			UnityEngine.InputSystem.InputSystem.QueueStateEvent<UnityEngine.InputSystem.LowLevel.MouseState>((InputDevice)null, (MouseState)new MouseState(), -1d);
			UnityEngine.InputSystem.InputSystem.QueueStateEvent((InputDevice)null, (KeyboardState)new KeyboardState(), -1d);
			UnityEngine.InputSystem.InputSystem.QueueStateEvent((InputDevice)null, (MouseState)new MouseState(), -1d);
			UnityEngine.InputSystem.InputSystem.QueueStateEvent<UnityEngine.InputSystem.LowLevel.MouseState>((InputDevice)null, (MouseState)new MouseState(), -1d);
			UnityEngine.InputSystem.InputSystem.RegisterLayout("","",null);
			UnityEngine.InputSystem.InputSystem.AddDevice("","", null);
        	UnityEngine.InputSystem.InputSystem.AddDevice<InputDevice>();
	        UnityEngine.InputSystem.InputSystem.AddDevice((InputDevice)null);
			UnityEngine.InputSystem.InputSystem.AddDevice(new UnityEngine.InputSystem.Layouts.InputDeviceDescription());
			UnityEngine.InputSystem.InputSystem.AddDevice(string.Empty);
        	UnityEngine.InputSystem.InputSystem.AddDeviceUsage(null, "");
			UnityEngine.InputSystem.InputSystem.RemoveLayout("");
			UnityEngine.InputSystem.InputSystem.GetDevice<InputDevice>("");
			UnityEngine.InputSystem.InputSystem.GetDevice(string.Empty);
			UnityEngine.InputSystem.InputSystem.GetDevice(typeof(InputDevice));
#endif

#if UNITY_2022 && ENABLE_IL2CPP && ENABLE_LEGACY_INPUT_MANAGER
			Input.GetKey("");
			Input.GetKeyDown("");
			Input.GetKeyUp("");
			Input.GetAxis("");
			Input.GetAxisRaw("");
			Input.GetButton("");
			Input.GetButtonDown("");
			Input.GetButtonUp("");
#endif

#if !UNITY_2019
		UnityEngine.ImageConversion.LoadImage(null, null);
		List<UnityEngine.UIElements.VisualElement> uiList = new List<UnityEngine.UIElements.VisualElement>();
		UnityEngine.UIElements.VisualElement ve = new UnityEngine.UIElements.VisualElement();
		if (ve != null)
		{
			UnityEngine.UIElements.UQueryExtensions.Query(ve, "", "").ToList(uiList);
			//ve?.Query("", "").ToList(uiList);
			UnityEngine.UIElements.UQueryExtensions.Query(ve, "", "", "").ToList(uiList);
			UnityEngine.UIElements.UQueryExtensions.Query(ve).ToList(uiList);
			UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.VisualElement>(null, "");
			UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.VisualElement>(null, "", "");

#if !UNITY_2020 || GDIO_UNITY_UIELEMENTS_2020
			UnityEngine.UIElements.RuntimePanelUtils.ScreenToPanel(ve.panel, new Vector2(1, 1));
#endif
		}
#endif
	}
}