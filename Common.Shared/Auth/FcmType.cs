using System.ComponentModel;
using System.Runtime.Serialization;

namespace Common.Shared.Auth;

public enum FcmType
{
    [EnumMember(Value = "data")]
    [Description("data")]
    Data,

    [EnumMember(Value = "signOut")]
    [Description("signOut")]
    SignOut,

    [EnumMember(Value = "enabled")]
    [Description("enabled")]
    Enabled,

    [EnumMember(Value = "disabled")]
    [Description("disabled")]
    Disabled,
}
