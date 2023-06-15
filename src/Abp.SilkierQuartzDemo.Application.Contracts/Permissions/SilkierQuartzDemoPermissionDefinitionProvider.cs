using Abp.SilkierQuartzDemo.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Abp.SilkierQuartzDemo.Permissions;

public class SilkierQuartzDemoPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(SilkierQuartzDemoPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(SilkierQuartzDemoPermissions.MyPermission1, L("Permission:MyPermission1"));

        myGroup.AddPermission("SilkierQuartz", L("Permission:SilkierQuartz"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SilkierQuartzDemoResource>(name);
    }
}
