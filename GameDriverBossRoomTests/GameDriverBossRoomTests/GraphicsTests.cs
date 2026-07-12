using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

/// <summary>
/// Graphics domain: the Settings panel's quality-cycle button (QualityButton.cs),
/// which steps through UnityEngine.QualitySettings levels and updates its own label.
///
/// Anchored by component type, not name -- QualityButton's GameObject is literally
/// named "Settings Button" (a copy-paste artifact of the shared button template),
/// the same name as the real gear icon that opens this panel. See BossRoomFlow for
/// the full landmine writeup.
/// </summary>
[TestFixture]
[Category("Graphics")]
public class GraphicsTests : GameDriverTest
{
    private const string QualityButton =
        "//*[fn:component('Unity.BossRoom.Gameplay.UI.QualityButton')]" +
        "/fn:component('Unity.BossRoom.Gameplay.UI.QualityButton')";
    private const string QualityLabelText = $"{QualityButton}/@m_QualityBtnText/@text";

    [OneTimeSetUp]
    public void GoToMainMenu() => BossRoomFlow.EnsureAtMainMenu(api);

    [SetUp]
    public void OpenSettings() => BossRoomFlow.OpenSettingsPanel(api);

    [TearDown]
    public void CloseSettings() => BossRoomFlow.CloseSettingsPanel(api);

    [Test]
    [Order(010)]
    public void T010_GivenSettingsOpen_QualityLabelShowsAValidLevel()
    {
        var label = api.GetObjectFieldValue<string>(QualityLabelText, 30);
        Assert.That(label, Is.Not.Null.And.Not.Empty,
            "Quality button label was empty; QualityButton.Start() should set it to the current level's name");
    }

    [Test]
    [Order(020)]
    public void T020_GivenSettingsOpen_WhenCyclingQuality_LabelChanges()
    {
        var before = api.GetObjectFieldValue<string>(QualityLabelText, 30);

        api.CallMethod(QualityButton, "SetQualitySettings", null);
        api.Wait(200);

        var after = api.GetObjectFieldValue<string>(QualityLabelText, 30);
        Assert.That(after, Is.Not.EqualTo(before),
            "Quality label did not change after cycling (SetQualitySettings should advance or wrap the level)");
    }
}
