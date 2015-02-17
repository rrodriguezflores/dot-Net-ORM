using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace rirf.orm.Dao
{
    /****************************************************************************************************/
    /* Clave del programa:  Parameter                                                                   */
    /* Creado por: Ricardo Israel Rodriguez Flores                                                      */
    /* Fecha: 21/Enero/2010                                                                             */
    /* Descripción: Clase que expone la immplementacion de un parametro que sera agregado a un DbCommand*/
    /* Fecha Modificación:                                                                              */
    /* Modificado por:                                                                                  */
    /* Descripción del cambio:                                                                          */
    /****************************************************************************************************/

    /// <summary>
    /// Clase que expone la immplementacion de un parametro que sera agregado a un DbCommand
    /// </summary>
    public class Parameter
    {
        public Parameter() 
        {
            this.sourceVersion = DataRowVersion.Current;
        }

        /// <summary>
        /// Nombre del parametro
        /// </summary>
        private String nombre;

        /// <summary>
        /// Tipo de dato asignado al parametro
        /// </summary>
        private DbType tipoDato;

        /// <summary>
        /// Direccion del parametro
        /// </summary>
        private ParameterDirection direccion;

        /// <summary>
        /// 
        /// </summary>
        private String sourceColumn;

        /// <summary>
        /// 
        /// </summary>
        private DataRowVersion sourceVersion;

        /// <summary>
        /// Valor asignado al parametro
        /// </summary>
        private Object value;

        /// <summary>
        /// Longitud del valor asignado al parametro
        /// </summary>
        private Int32 size;

        /// <summary>
        /// Bandera que indica si el parametro puede contener valores nulos
        /// </summary>
        private bool nullable;

        /// <summary>
        /// 
        /// </summary>
        private byte precision;
        
        /// <summary>
        /// 
        /// </summary>
        private byte scale;

        /// <summary>
        /// Asigna o devuelve el nombre del parametro
        /// </summary>
        public String Nombre
        {
            get { return nombre; }
            set { nombre = value; }
        }

        /// <summary>
        /// Asigna o devuelve el tipo de dato del parametro
        /// </summary>
        public DbType TipoDato
        {
            get { return tipoDato; }
            set { tipoDato = value; }
        }

        /// <summary>
        /// Asigna o devuelve la direccion del parametro
        /// </summary>
        public ParameterDirection Direccion
        {
            get { return direccion; }
            set { direccion = value; }
        }

        /// <summary>
        /// Asigna o devuelve el 
        /// </summary>
        public String SourceColumn
        {
            get { return sourceColumn; }
            set { sourceColumn = value; }
        }

        /// <summary>
        /// Asigna o devuelve el 
        /// </summary>
        public DataRowVersion SourceVersion
        {
            get { return sourceVersion; }
            set { sourceVersion = value; }
        }

        /// <summary>
        /// Asigna o devuelve el valor asociado al parametro
        /// </summary>
        public Object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Asigna o devuelve la longitus del valor asignado al parametro
        /// </summary>
        public Int32 Size
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>
        /// Asigna o devuelve la bandera que indica si el valor del parametro puede ser nulo o no
        /// </summary>
        public bool Nullable
        {
            get { return nullable; }
            set { nullable = value; }
        }

        /// <summary>
        /// Asigna o devuelve la presicion del valor asignado al parametro
        /// </summary>
        public byte Precision
        {
            get { return precision; }
            set { precision = value; }
        }


        public byte Scale
        {
            get { return scale; }
            set { scale = value; }
        }
    }
}
