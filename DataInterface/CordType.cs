using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataInterface {
    public class CordType : IEquatable<CordType> {

        public CordType(short? x, short? y) {
            if (x != null && y != null) {
                this._x = (short)x;
                this._y = (short)y;
            }
        }

        private short _x;
        /// <summary>
        /// The STDF x
        /// </summary>
        public short CordX {
            get { return _x; }
        }

        private short _y;
        /// <summary>
        /// The STDF y
        /// </summary>
        public short CordY {
            get { return _y; }
        }


        /// <summary>
        /// Overrides <see cref="Object.Equals(object)"/> appropriately
        /// </summary>
        /// <param name="obj">the object to compare to</param>
        /// <returns>true if the instance is equal to <paramref name="obj"/>, otherwise false</returns>
        public override bool Equals(object obj) {
            if (!(obj is CordType)) {
                return false;
            }
            return Equals((CordType)obj);
        }

        /// <summary>
        /// Gets an appropriate hash code for this instance
        /// </summary>
        public override int GetHashCode() {
            return ((ushort)_x << 16) | (ushort)_y;
        }

        /// <summary>
        /// Supplies an appropriate string representation of this instance.
        /// </summary>
        public override string ToString() {
            return string.Format("X:{0} Y:{1}", this._x, this._y);
        }

        #region IEquatable<CordType> Members

        /// <summary>
        /// Implements equality
        /// </summary>
        /// <param name="other">the CordType to compare to</param>
        /// <returns>true if the instance is equal to <paramref name="other"/>, otherwise false</returns>
        public bool Equals(CordType other) {
            return (this._x == other._x && this._y == other._y);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(CordType first, CordType second) {
            return first.Equals(second);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(CordType first, CordType second) {
            return !first.Equals(second);
        }
        #endregion
    }
}
