using System;

namespace PI.GestaoHospitalar.Core.Helpers
{
    public static class ExtensionHelper
    {
        public static string GetTableName(this Type type)
        {
            return $"{type.Namespace.Replace(".", "_")}_{type.Name}";
        }       
    }
}
