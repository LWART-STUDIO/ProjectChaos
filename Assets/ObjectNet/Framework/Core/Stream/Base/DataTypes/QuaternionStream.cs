using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A data handler for handling Quaternion data streams.
    /// </summary>
    public class QuaternionStream : DataHandler<Quaternion> {
        /// <summary>
        /// Writes the Quaternion data to the byte buffer at the specified offset.
        /// </summary>
        /// <param name="data">The Quaternion data to write.</param>
        /// <param name="buffer">The byte buffer to write to.</param>
        /// <param name="offset">The offset in the buffer to start writing at.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        public override int Write(Quaternion data, ref byte[] buffer, ref int offset) {
            int result = base.Write(data.x, ref buffer, ref offset, typeof(float));
            result += base.Write(data.y, ref buffer, ref offset, typeof(float));
            result += base.Write(data.z, ref buffer, ref offset, typeof(float));
            result += base.Write(data.w, ref buffer, ref offset, typeof(float));
            return result;
        }

        /// <summary>
        /// Reads Quaternion data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The Quaternion data read from the buffer.</returns>
        public override Quaternion Read(byte[] buffer, ref int offset) {
            return new Quaternion(this.Read<float>(buffer, ref offset),
                                  this.Read<float>(buffer, ref offset),
                                  this.Read<float>(buffer, ref offset),
                                  this.Read<float>(buffer, ref offset));
        }
    }
}