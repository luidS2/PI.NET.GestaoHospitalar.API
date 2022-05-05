namespace PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.Queries
{
    public class PacienteQuery
    {
        internal string Merge()
        {
            return @"
                MERGE 
                    {0} AS Destino
                USING 
                    {1} AS Origem ON (Origem.Codigo = Destino.Codigo)
                
                WHEN MATCHED THEN
                    UPDATE SET 
                         Destino.Codigo			            = Origem.Codigo
                        ,Destino.CodigoFornecedor     	    = Origem.CodigoFornecedor
                        ,Destino.SiglaEstado	            = Origem.SiglaEstado
                        ,Destino.ChaveLocal				    = Origem.ChaveLocal
                        ,Destino.NomeFantasia      		    = Origem.NomeFantasia
                        ,Destino.RazaoSocial           	    = Origem.RazaoSocial
                        ,Destino.TipoLogradouro             = Origem.TipoLogradouro
                        ,Destino.NomeLogradouro             = Origem.NomeLogradouro
                        ,Destino.NumeroLogradouro           = Origem.NumeroLogradouro
                        ,Destino.Complemento                = Origem.Complemento
                        ,Destino.Bairro                     = Origem.Bairro
                        ,Destino.CEP                        = Origem.CEP
                        ,Destino.DDD1                       = Origem.DDD1
                        ,Destino.Telefone1                  = Origem.Telefone1
                        ,Destino.DDD2                       = Origem.DDD2
                        ,Destino.Telefone2                  = Origem.Telefone2
                        ,Destino.DDD3                       = Origem.DDD3
                        ,Destino.TelefoneFax                = Origem.TelefoneFax
                        ,Destino.Email                      = Origem.Email
                        ,Destino.CNPJ                       = Origem.CNPJ
                        ,Destino.InscricaoEstadual          = Origem.InscricaoEstadual
                        ,Destino.CodigoInspecaoTransporte   = Origem.CodigoInspecaoTransporte
                        ,Destino.CodigoIBGE                 = Origem.CodigoIBGE
                        ,Destino.DigitoIBGE                 = Origem.DigitoIBGE
                        ,Destino.CodigoBairro               = Origem.CodigoBairro
                        ,Destino.CodigoLogradouro           = Origem.CodigoLogradouro
                
                WHEN NOT MATCHED THEN
                    INSERT
                    VALUES(Origem.Id, Origem.Codigo, Origem.CodigoFornecedor, Origem.SiglaEstado, Origem.ChaveLocal, Origem.NomeFantasia, Origem.RazaoSocial, Origem.TipoLogradouro,
                           Origem.NomeLogradouro, Origem.NumeroLogradouro, Origem.Complemento, Origem.Bairro, Origem.CEP, Origem.DDD1, Origem.Telefone1, Origem.DDD2, Origem.Telefone2,
                           Origem.DDD3, Origem.TelefoneFax, Origem.Email, Origem.CNPJ, Origem.InscricaoEstadual, Origem.CodigoInspecaoTransporte, Origem.CodigoIBGE, Origem.DigitoIBGE,
                           Origem.CodigoBairro, Origem.CodigoLogradouro);
            ";
        }
    }
}
