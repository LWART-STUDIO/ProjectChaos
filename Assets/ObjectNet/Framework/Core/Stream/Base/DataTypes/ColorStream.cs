using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A specialized data handler for streaming Color objects to and from byte arrays.
    /// </summary>
    public class ColorStream : DataHandler<Color> {

        /// <summary>
        /// Writes a Color object to a byte array buffer.
        /// </summary>
        /// <param name="data">The Color object to write.</param>
        /// <param name="buffer">The byte array buffer to write to.</param>
        /// <param name="offset">The current offset in the buffer. Will be updated after write.</param>
        /// <returns>The number of bytes written.</returns>
        public override int Write(Color data, ref byte[] buffer, ref int offset) {
            // Convert the Color object to a string with a fixed-point notation.
            string colorString = data.ToString("F5");
            // Write the string representation of the Color object to the buffer using the base class method.
            return base.Write(colorString, ref buffer, ref offset, typeof(string));
        }

        /// <summary>
        /// Reads a Color object from a byte array buffer.
        /// </summary>
        /// <param name="buffer">The byte array buffer to read from.</param>
        /// <param name="offset">The current offset in the buffer. Will be updated after read.</param>
        /// <returns>The Color object read from the buffer.</returns>
        public override Color Read(byte[] buffer, ref int offset) {
            // Read the string representation of the Color object from the buffer.
            string readData = this.Read<string>(buffer, ref offset);
            // Extract the RGBA components from the string and split them into an array.
            string[] rgba = readData.Substring(5, readData.Length - 6).Split(", ");
            // Parse the RGBA components and create a new Color object.
            Color color = new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));
            // Return the Color object.
            return color;
        }
    }

}