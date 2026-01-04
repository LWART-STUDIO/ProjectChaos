using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A data handler for handling Vector3 data streams.
    /// </summary>
    public class Vector3Stream : DataHandler<Vector3> {
        /// <summary>
        /// Writes the Vector3 data to the byte buffer at the specified offset.
        /// </summary>
        /// <param name="data">The Vector3 data to write.</param>
        /// <param name="buffer">The byte buffer to write to.</param>
        /// <param name="offset">The offset in the buffer to start writing at.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        public override int Write(Vector3 data, ref byte[] buffer, ref int offset) {
            int result = base.Write(data.x, ref buffer, ref offset, typeof(float));
            result += base.Write(data.y, ref buffer, ref offset, typeof(float));
            result += base.Write(data.z, ref buffer, ref offset, typeof(float));
            return result;
        }

        /// <summary>
        /// Reads Vector3 data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The Vector3 data read from the buffer.</returns>
        public override Vector3 Read(byte[] buffer, ref int offset) {
            return new Vector3(this.Read<float>(buffer, ref offset),
                               this.Read<float>(buffer, ref offset),
                               this.Read<float>(buffer, ref offset));
        }
    }
}