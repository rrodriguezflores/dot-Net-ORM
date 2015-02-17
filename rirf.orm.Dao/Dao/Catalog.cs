using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rirf.orm.Dao.Attributes;
using System.Data;

namespace rirf.orm.Dao
{
    /****************************************************************************************************/
    /* Clave del programa:  Catalog                                                                     */
    /* Creado por: Ricardo Israel Rodriguez Flores                                                      */
    /* Fecha: 03/Diciembre/2009                                                                         */
    /* Descripción: Clase Base a partir de la cual heredan propiedades todos los objetos considerados   */
    /*              como catalogos                                                                      */
    /* Fecha Modificación:                                                                              */
    /* Modificado por: Ricardo Israel Rodriguez Flores                                                  */
    /* Descripción del cambio:                                                                          */
    /****************************************************************************************************/

    /// <summary>
    /// Clase Base a partir de la cual heredan propiedades todos los objetos considerados como catalogos
    /// </summary>
    public abstract class Catalog : Entity
    {
        /// <summary>
        /// Columna que indica si el registro se encuentra activo o no
        /// </summary>
        [Column("N_ESTATUS", DbType.Int32)]
        protected Int32 estatus;

        /// <summary>
        /// Asigna o devuelve el valor del estatus del registro
        /// </summary>
        public Int32 Estatus
        {
            get { return estatus; }
            set { estatus = value; }
        }
    }
}
