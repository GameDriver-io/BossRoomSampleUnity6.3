using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

/// <summary>
/// Audio domain: theme music playback and the Settings panel's volume sliders.
/// Settings panel open/close lives in <see cref="BossRoomFlow"/> so Graphics tests
/// reuse it too; see that file for why the panel is anchored by component type.
/// </summary>
[TestFixture]
[Category("Audio")]
public class AudioTests : GameDriverTest
{
    private const string MusicPlayerSource =
        "//*[@name='ClientMusicPlayer']/fn:component('UnityEngine.AudioSource')";

    private const string SettingsPanel =
        "//*[fn:component('Unity.BossRoom.Gameplay.UI.UISettingsPanel')]" +
        "/fn:component('Unity.BossRoom.Gameplay.UI.UISettingsPanel')";
    private const string MasterVolumeSlider = $"{SettingsPanel}/@m_MasterVolumeSlider";
    private const string MusicVolumeSlider = $"{SettingsPanel}/@m_MusicVolumeSlider";

    [OneTimeSetUp]
    public void GoToMainMenu() => BossRoomFlow.EnsureAtMainMenu(api);

    [Test]
    [Order(010)]
    public void T010_GivenMainMenu_ThemeMusicIsPlaying()
    {
        Assert.That(api.GetObjectFieldValue<bool>($"{MusicPlayerSource}/@isPlaying", 30), Is.True,
            "Theme music was not playing on the MainMenu (MainMenuMusicStarter should have triggered it)");
    }

    [Test]
    [Order(020)]
    public void T020_GivenMainMenu_WhenOpeningSettings_VolumeSlidersInValidRange()
    {
        // ClientPrefs defaults: master 0.5, music 0.8 -- but a prior run may have changed
        // and persisted these, so assert the valid [0,1] range rather than an exact value.
        BossRoomFlow.OpenSettingsPanel(api);

        var masterVolume = api.GetObjectFieldValue<float>($"{MasterVolumeSlider}/@value", 30);
        var musicVolume = api.GetObjectFieldValue<float>($"{MusicVolumeSlider}/@value", 30);

        Assert.That(masterVolume, Is.InRange(0f, 1f), "Master volume slider out of range");
        Assert.That(musicVolume, Is.InRange(0f, 1f), "Music volume slider out of range");

        BossRoomFlow.CloseSettingsPanel(api);
    }

    [Test]
    [Order(030)]
    public void T030_GivenSettingsOpen_WhenMasterVolumeSliderChanged_NewValueSticks()
    {
        BossRoomFlow.OpenSettingsPanel(api);

        var original = api.GetObjectFieldValue<float>($"{MasterVolumeSlider}/@value", 30);
        var target = original > 0.5f ? 0.1f : 0.9f;

        // Slider.value's setter invokes onValueChanged (ClientPrefs.SetMasterVolume +
        // AudioMixerConfigurator.Configure()), the same as if a player dragged it.
        api.SetObjectFieldValue(MasterVolumeSlider, "value", target);
        api.Wait(200);

        var updated = api.GetObjectFieldValue<float>($"{MasterVolumeSlider}/@value", 30);
        Assert.That(updated, Is.EqualTo(target).Within(0.01f),
            "Master volume slider did not retain the value it was set to");

        // Restore so the fixture leaves the game state as it found it.
        api.SetObjectFieldValue(MasterVolumeSlider, "value", original);
        BossRoomFlow.CloseSettingsPanel(api);
    }
}
