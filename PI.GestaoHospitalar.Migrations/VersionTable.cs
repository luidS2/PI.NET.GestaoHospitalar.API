using FluentMigrator.Runner.VersionTableInfo;

namespace PI.GestaoHospitalar.Migrations
{
    [VersionTableMetaData]

    public class VersionTable : IVersionTableMetaData
    {
        public string ColumnName
        {
            get { return "Versao"; }
        }

        public string SchemaName
        {
            get { return ""; }
        }

        public string TableName
        {
            get { return $"{typeof(VersionTable).Namespace.Replace(".Migrations", "").Replace(".", "_")}_Info"; }
        }

        public string UniqueIndexName
        {
            get { return "UC_Version"; }
        }

        public virtual string AppliedOnColumnName
        {
            get { return "DataAplicacaoVersao"; }
        }

        public virtual string DescriptionColumnName
        {
            get { return "Descricao"; }
        }

        public object ApplicationContext { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public bool OwnsSchema => false;
    }
}
