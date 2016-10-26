using UnityEngine;


public class MSAccessPortMod : FortressCraftMod
{
    public ushort AccessPortType = ModManager.mModMappings.CubesByKey["steveman0.MSAccessPort"].CubeType;

    public override ModRegistrationData Register()
    {
        ModRegistrationData modRegistrationData = new ModRegistrationData();
        modRegistrationData.RegisterEntityHandler("steveman0.MSAccessPort");
        modRegistrationData.RegisterEntityUI("steveman0.MSAccessPort", new MSAccessPortWindow());

        Debug.Log("Mass Storage Access Port Mod v6 registered");

        UIManager.NetworkCommandFunctions.Add("steveman0.MSAccessPortWindow", new UIManager.HandleNetworkCommand(MSAccessPortWindow.HandleNetworkCommand));

        return modRegistrationData;
    }

    public override ModCreateSegmentEntityResults CreateSegmentEntity(ModCreateSegmentEntityParameters parameters)
    {
        ModCreateSegmentEntityResults result = new ModCreateSegmentEntityResults();

        if (parameters.Cube == AccessPortType)
        {
            parameters.ObjectType = SpawnableObjectEnum.RefineryController;
            result.Entity = new MSAccessPort(parameters);
        }
        return result;
    }
}
