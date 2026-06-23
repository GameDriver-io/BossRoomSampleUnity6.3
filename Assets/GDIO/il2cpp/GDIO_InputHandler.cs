using static gdio.unity_agent.recorder.AgentInterface;

namespace gdio.unity_agent
{
    public static class GDIO_InputHandler
    {
        public static PlayerSettingsActiveInputHandler ActiveInputHandler
        {
            get
            {
                string backend = gdio.unity_agent.Utilities.ConfigHelper.GetConfigItemValueOrDefault("/config/project/@backend", "").ToLower();
                if (backend.Length != 0)
                {
                    if (backend == "new" || backend == "new input")
                        return PlayerSettingsActiveInputHandler.New;
                    else if (backend == "both")
                        return PlayerSettingsActiveInputHandler.Both;
                    else
                        return PlayerSettingsActiveInputHandler.Legacy;
                }

#if GDIO_ACTIVEINPUTHANDLER_OLD
                return PlayerSettingsActiveInputHandler.Legacy;
#elif GDIO_ACTIVEINPUTHANDLER_NEW
                return PlayerSettingsActiveInputHandler.New;
#elif GDIO_ACTIVEINPUTHANDLER_BOTH
                return PlayerSettingsActiveInputHandler.Both;
#else
                return PlayerSettingsActiveInputHandler.Legacy;
#endif
            }
        }
    }
}