using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace PI.GestaoHospitalar.Core.Util
{
    public static class SafeConvert
    {
        /// <summary>
        /// Tenta convertar o valor informado para um System.Int32. Caso não seja possível devolve o valor informado
        /// em defaultValue.
        /// </summary>
        /// <param name="value">Valor a ser convertido para inteiro.</param>
        /// <param name="defaultValue">Valor convertido ou defaultValue caso não seja possível executar a conversão.</param>
        /// <returns></returns>
        public static int ToInt32(object value, int defaultValue)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Tenta converter o valor informado para um System.Int32. Caso não seja possível devolve default(int).
        /// </summary>
        /// <param name="value">Valor a ser convertido para inteiro.</param>
        /// <returns>Valor convertido ou  default(int) caso não seja possivel executar a conversão.</returns>
        public static int ToInt32(object value)
        {
            return ToInt32(value, default);
        }


        /// <summary>
        /// Tenta convertar o valor informado para um System.Int16. Caso não seja possível devolve o valor informado
        /// em defaultValue.
        /// </summary>
        /// <param name="value">Valor a ser convertido para short.</param>
        /// <param name="defaultValue">Valor convertido ou defaultValue caso não seja possível executar a conversão.</param>
        /// <returns></returns>
        public static short ToInt16(object value, short defaultValue)
        {
            try
            {
                return Convert.ToInt16(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Tenta converter o valor informado para um System.Int16. Caso não seja possível devolve default(short).
        /// </summary>
        /// <param name="value">Valor a ser convertido para short.</param>
        /// <returns>Valor convertido ou  default(short) caso não seja possivel executar a conversão.</returns>
        public static short ToInt16(object value)
        {
            return ToInt16(value, default);
        }


        /// <summary>
        /// Tenta convertar o valor informado para um System.String. Caso não seja possível devolve o valor informado
        /// em defaultValue.
        /// </summary>
        /// <param name="value">Valor a ser convertido para string.</param>
        /// <param name="defaultValue">Valor convertido ou defaultValue caso não seja possível executar a conversão.</param>
        /// <returns></returns>
        public static string ToString(object value, string defaultValue)
        {
            return (value == null ? defaultValue : Convert.ToString(value));
        }

        /// <summary>
        /// Tenta converter o valor informado para um System.String. Caso não seja possível devolve string.Emtpy.
        /// </summary>
        /// <param name="value">Valor a ser convertido para string.</param>
        /// <returns>Valor convertido ou string.Empty caso não seja possivel executar a conversão.</returns>
        public static string ToString(object value)
        {
            return ToString(value, string.Empty);
        }

        /// <summary>
        /// Tenta realizar o typecast do objeto para o tipo informado.
        /// </summary>
        /// <typeparam name="T">Tipo para o qual o objeto será convertido.</typeparam>
        /// <param name="value">Objeto a ser convertido.</param>
        /// <param name="result">Parâmetro de saida com o tipo convertido caso a conversão seja possível.</param>
        /// <returns>Verdadeiro caso a conversão seja bem sucessida, caso contrário falso.</returns>
        public static bool TryCast<T>(object value, out T result)
        {
            if (value != null)
            {
                if (typeof(IConvertible).IsAssignableFrom(value.GetType()))
                {
                    result = (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
                    return true;
                }
                else if (value is T)
                {
                    result = (T)value;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public static DataTable ToDataTable<T>(this IList<T> data, string name)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(values);
            }
            return table;
        }

        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            return ToDataTable(data, "");
        }
    }
}
