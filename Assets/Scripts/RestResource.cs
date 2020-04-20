public class RestResource : BaseResource
{
    override public bool IsConsumable { get { return true; } }
    override public Resource ResourceType { get { return Resource.Rest; } }
    override public void Consume() { }
    override public void RainedOn() { }
    public override void SetAsNotConsumable() { }
    override public void StruckedByLightning() { }
}
