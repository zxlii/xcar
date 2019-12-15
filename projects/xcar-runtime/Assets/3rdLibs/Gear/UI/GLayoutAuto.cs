namespace Gear.Runtime.UI
{
public class GLayoutAuto : GLayout
{
    protected override void OnEnable()
    {
        LayoutChange();
    }
    protected override void OnDisable()
    {
        LayoutChange();
    }
    protected override void OnRectTransformDimensionsChange()
    {
        LayoutChange();
    }
    protected virtual void OnTransformChildrenChanged()
    {
        LayoutChange();
    }
}
}