using Menu.Remix.MixedUI;
using UnityEngine;

namespace TestMod;

public sealed class ModOptions : OptionsTemplate
{
    public static readonly ModOptions Instance = new();

    public readonly Color WarnRed = new(0.85f, 0.35f, 0.4f);

    public static Configurable<bool> TestOption = Instance.config.Bind(nameof(TestOption), true, new ConfigurableInfo(
        "Test Description.", null, "",
        "Test Option"));

    public const int TAB_COUNT = 1;

    public override void Initialize()
    {
        base.Initialize();

        Tabs = new OpTab[TAB_COUNT];
        int tabIndex = -1;

        InitGeneral(ref tabIndex);
    }


    private void InitGeneral(ref int tabIndex)
    {
        AddTab(ref tabIndex, "General");

        AddCheckBox(TestOption);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(21);
        DrawBox(ref Tabs[tabIndex]);

        if (GetConfigurable(TestOption, out OpCheckBox checkBox))
            checkBox.colorEdge = WarnRed;

        if (GetLabel(TestOption, out var label))
            label.color = WarnRed;
    }
}