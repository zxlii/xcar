namespace Gear.Runtime
{
    public enum UVFillMethod
    {
        SymmetryLR,     //���ҶԳ�
        SymmetryUD,     //���¶Գ�
        SymmetryLRUD,    //�������ҶԳ�
        None
    }

    public enum SortType
    {
        Position,
        Priority,
        Sibling
    }
    public enum Align
    {
        Top = 1,
        Bottom,
        Center,
        Left,
        Right
    }
    public enum PanelCloseType
    {
        ClickAnyWhere,
        ClickEmpty,
        None
    }

    public enum PanelLayer
    {
        GameWorld,
        RootPanel,
        SubPanel,
        NormalTip,
        ImportantTip,
        Debug
    }

    public enum PanelLayerType
    {
        FixedLayer,
        AlwaysOnTopOfAll,
        AlwaysOnTopOfCurrent
    }

}