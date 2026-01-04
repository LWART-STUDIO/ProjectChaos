
using System;
using System.Text;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A generic data handler class that provides serialization and deserialization functionality for various data types.
    /// It implements both IDataWritter and IDataReader interfaces for writing and reading data respectively.
    /// </summary>
    /// <typeparam name="T">The type of data to handle.</typeparam>
    public class DataHandler<T> : IDataWritter<T>, IDataReader<T> {

        /// <summary>
        /// Writes data of type T to a buffer at a specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data to.</param>
        /// <param name="offset">The offset in the buffer at which to start writing.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        public virtual int Write(T data, ref byte[] buffer, ref int offset) {
            return this.WriteData(data, ref buffer, ref offset, typeof(T));
        }

        /// <summary>
        /// Writes data of a specified type to a buffer at a specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data to.</param>
        /// <param name="offset">The offset in the buffer at which to start writing.</param>
        /// <param name="dataType">The type of the data to write.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        public virtual int Write(object data, ref byte[] buffer, ref int offset, Type dataType) {
            return this.WriteData(data, ref buffer, ref offset, dataType);
        }

        /// <summary>
        /// Reads data of type T from a buffer at a specified offset.
        /// </summary>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="offset">The offset in the buffer at which to start reading.</param>
        /// <returns>The data read from the buffer.</returns>
        public virtual T Read(byte[] buffer, ref int offset) {
            return this.ReadData(buffer, ref offset, typeof(T));
        }

        /// <summary>
        /// Reads data of a specified type from a buffer at a specified offset.
        /// </summary>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="offset">The offset in the buffer at which to start reading.</param>
        /// <param name="dataType">The type of the data to read.</param>
        /// <returns>The data read from the buffer.</returns>
        public virtual T Read(byte[] buffer, ref int offset, Type dataType) {
            return this.ReadData(buffer, ref offset, dataType);
        }

        /// <summary>
        /// Reads data of type E from a buffer at a specified offset.
        /// </summary>
        /// <typeparam name="E">The type of data to read.</typeparam>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="offset">The offset in the buffer at which to start reading.</param>
        /// <returns>The data read from the buffer.</returns>
        public virtual E Read<E>(byte[] buffer, ref int offset) {
            return (E)this.ReadData<E>(buffer, ref offset, typeof(E));
        }

        /// <summary>
        /// Reads data of type E from a buffer at a specified offset, using a specified data type.
        /// </summary>
        /// <typeparam name="E">The type of data to read.</typeparam>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="offset">The offset in the buffer at which to start reading.</param>
        /// <param name="dataType">The type of the data to read.</param>
        /// <returns>The data read from the buffer.</returns>
        public virtual E Read<E>(byte[] buffer, ref int offset, Type dataType) {
            return (E)this.ReadData<E>(buffer, ref offset, dataType);
        }

        /// <summary>
        /// Writes the given data into the buffer at the specified offset and updates the offset.
        /// </summary>
        /// <param name="data">The data to write into the buffer.</param>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The current offset in the buffer. This will be updated after writing.</param>
        /// <param name="dataType">The type of the data to be written.</param>
        /// <returns>The size of the data written to the buffer.</returns>
        /// <exception cref="Exception">Thrown when the buffer does not have enough space to write the data.</exception>
        private int WriteData(object data, ref byte[] buffer, ref int offset, Type dataType) {
            int writeSize = 0;
            if ((offset + DataUtils.SizeOfType(dataType)) <= buffer.Length) {
                if (dataType == typeof(string)) {
                    byte[] bytesToWrite = this.ToByteArray(data);
                    byte[] sizeBytes = this.ToByteArray(bytesToWrite.Length);
                    // Write size
                    Array.Copy(sizeBytes, 0, buffer, offset, sizeBytes.Length);
                    // write data
                    Array.Copy(bytesToWrite, 0, buffer, offset + sizeBytes.Length, bytesToWrite.Length);
                    // Total of bytes written
                    writeSize = (bytesToWrite.Length + sizeBytes.Length);
                    // Increment offset
                    offset += writeSize;
                } else if (dataType == typeof(byte[])) {
                    byte[] bytesToWrite = ((byte[])data);
                    byte[] sizeBytes = this.ToByteArray(bytesToWrite.Length);
                    // Write size
                    Array.Copy(sizeBytes, 0, buffer, offset, sizeBytes.Length);
                    // write data
                    Array.Copy(bytesToWrite, 0, buffer, offset + sizeBytes.Length, bytesToWrite.Length);
                    // Total of bytes written
                    writeSize = (bytesToWrite.Length + sizeBytes.Length);
                    // Increment offset
                    offset += writeSize;
                } else {
                    writeSize = DataUtils.SizeOfType(dataType);
                    Array.Copy(this.ToByteArray(data), 0, buffer, offset, writeSize);
                    offset += writeSize;
                }
            } else {
                throw new Exception(String.Format("Buffer has no avaiable space to write \"{0}\", required {1} avaiable {2}", dataType.Name, (offset + DataUtils.SizeOfType(dataType)), buffer.Length));
            }
            return writeSize;
        }

        /// <summary>
        /// Reads data of a specified type from the buffer at the given offset and updates the offset.
        /// </summary>
        /// <typeparam name="T">The type of the data to be read.</typeparam>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="offset">The current offset in the buffer. This will be updated after reading.</param>
        /// <param name="dataType">The type of the data to be read.</param>
        /// <returns>The data read from the buffer.</returns>
        /// <exception cref="Exception">Thrown when the buffer does not have enough space to read the data.</exception>
        private T ReadData(byte[] buffer, ref int offset, Type dataType) {
            T result = default(T);
            if ((offset + DataUtils.SizeOfType(dataType)) <= buffer.Length) {
                if (dataType == typeof(string)) {
                    // first i need to read size
                    int totalOfBytesOnString = this.ReadData<int>(buffer, ref offset, typeof(int));
                    // Now read total of bytes
                    byte[] objectBytes = new byte[totalOfBytesOnString];
                    for (int index = 0; index < objectBytes.Length; index++) {
                        objectBytes[index] = buffer[offset++];
                    }
                    result = (T)this.FromByteArray(objectBytes);
                } else if (dataType == typeof(byte[])) {
                    // first i need to read size
                    int totalOfBytesOnArray = this.ReadData<int>(buffer, ref offset, typeof(int));
                    // Now read total of bytes
                    byte[] objectBytes = new byte[totalOfBytesOnArray];
                    for (int index = 0; index < objectBytes.Length; index++) {
                        objectBytes[index] = buffer[offset++];
                    }
                    result = (T)this.FromByteArray(objectBytes);
                } else {
                    byte[] objectBytes = new byte[DataUtils.SizeOfType(dataType)];
                    for (int index = 0; index < objectBytes.Length; index++) {
                        objectBytes[index] = buffer[offset++];
                    }
                    result = (T)this.FromByteArray(objectBytes);
                }
            } else {
                throw new Exception(String.Format("Buffer has no avaiable space to read \"{0}\", required {1} avaiable {2}", dataType.Name, (offset + DataUtils.SizeOfType(dataType)), buffer.Length));
            }
            return result;
        }

        /// <summary>
        /// Reads data of a specified type from a buffer at a given offset.
        /// </summary>
        /// <typeparam name="T">The type of the data to read.</typeparam>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="offset">The offset at which to start reading.</param>
        /// <param name="dataType">The type of the data to read.</param>
        /// <returns>The data read from the buffer.</returns>
        /// <exception cref="Exception">Thrown when the buffer does not have enough space to read the data.</exception>
        private E ReadData<E>(byte[] buffer, ref int offset, Type dataType) {
            E result = default(E);
            if ((offset + DataUtils.SizeOfType(dataType)) <= buffer.Length) {
                if (dataType == typeof(string)) {
                    // first i need to read size
                    int totalOfBytesOnString = this.ReadData<int>(buffer, ref offset, typeof(int));
                    // Now read total of bytes
                    byte[] objectBytes = new byte[totalOfBytesOnString];
                    for (int index = 0; index < objectBytes.Length; index++) {
                        objectBytes[index] = buffer[offset++];
                    }
                    result = this.FromByteArray<E>(objectBytes, dataType);
                } else if (dataType == typeof(byte[])) {
                    // first i need to read size
                    int totalOfBytesOnArray = this.ReadData<int>(buffer, ref offset, typeof(int));
                    // Now read total of bytes
                    byte[] objectBytes = new byte[totalOfBytesOnArray];
                    for (int index = 0; index < objectBytes.Length; index++) {
                        objectBytes[index] = buffer[offset++];
                    }
                    result = this.FromByteArray<E>(objectBytes, dataType);
                } else {
                    byte[] objectBytes = new byte[DataUtils.SizeOfType(dataType)];
                    for ( int index = 0; index < objectBytes.Length; index++) {
                        objectBytes[index] = buffer[offset++];
                    }
                    result = this.FromByteArray<E>(objectBytes, dataType);
                }
            } else {
                throw new Exception(String.Format("Buffer has no avaiable space to read \"{0}\", required {1} avaiable {2}", dataType.Name, (offset + DataUtils.SizeOfType(dataType)), buffer.Length));
            }
            return result;
        }

        /// <summary>
        /// Converts an object to its byte array representation.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>A byte array representing the object.</returns>
        private byte[] ToByteArray(object obj) {
            byte[] result = null;
            if (obj == null) {
                return result;
            }
            switch(obj) {
                case int        : result = BitConverter.GetBytes((int)obj); break;
                case uint       : result = BitConverter.GetBytes((uint)obj); break;
                case ushort     : result = BitConverter.GetBytes((ushort)obj); break;
                case float      : result = BitConverter.GetBytes((float)obj); break;
                case long       : result = BitConverter.GetBytes((long)obj); break;
                case ulong      : result = BitConverter.GetBytes((ulong)obj); break;
                case short      : result = BitConverter.GetBytes((short)obj); break;
                case double     : result = BitConverter.GetBytes((double)obj); break;
                case char       : result = BitConverter.GetBytes((char)obj); break;
                case bool       : result = BitConverter.GetBytes((bool)obj); break;
                case string     : result = Encoding.ASCII.GetBytes((obj as string)); break;
                case sbyte      : result = new byte[] { (byte)((sbyte)obj & 0xFF)}; break;
                case byte       : result = new byte[] { (byte)obj }; break;
                case byte[]     : result = (byte[])obj; break;
                case Vector2    : result = this.Vector2ToBytes((Vector2)obj); break;
                case Vector3    : result = this.Vector3ToBytes((Vector3)obj); break;
                case Quaternion : result = this.QuaternionToBytes((Quaternion)obj); break;
                case Color      : result = this.ColorToBytes((Color)obj); break;
            }            
            return result;
        }

        /// <summary>
        /// Converts a byte array to an object of a specified type.
        /// </summary>
        /// <param name="data">The byte array to convert.</param>
        /// <returns>An object of type T represented by the byte array.</returns>
        private object FromByteArray(byte[] data) {
            object result = default(object);
            if (data == null) {
                return default(T);
            }            
            if ((typeof(T)) == typeof(int))             result = BitConverter.ToInt32(data);
            else if ((typeof(T)) == typeof(uint))       result = BitConverter.ToUInt32(data);
            else if ((typeof(T)) == typeof(ushort))     result = BitConverter.ToUInt16(data);
            else if ((typeof(T)) == typeof(float))      result = BitConverter.ToSingle(data);
            else if ((typeof(T)) == typeof(long))       result = BitConverter.ToInt64(data);
            else if ((typeof(T)) == typeof(ulong))      result = BitConverter.ToUInt64(data);
            else if ((typeof(T)) == typeof(short))      result = BitConverter.ToInt16(data);
            else if ((typeof(T)) == typeof(double))     result = BitConverter.ToDouble(data);
            else if ((typeof(T)) == typeof(char))       result = BitConverter.ToChar(data);
            else if ((typeof(T)) == typeof(bool))       result = BitConverter.ToBoolean(data);
            else if ((typeof(T)) == typeof(string))     result = Encoding.ASCII.GetString(data);
            else if ((typeof(T)) == typeof(sbyte))      result = (sbyte)data[0];
            else if ((typeof(T)) == typeof(byte))       result = data[0];
            else if ((typeof(T)) == typeof(byte[]))     result = data;
            else if ((typeof(T)) == typeof(Vector2))    result = this.BytesToVector2(data);
            else if ((typeof(T)) == typeof(Vector3))    result = this.BytesToVector3(data);
            else if ((typeof(T)) == typeof(Quaternion)) result = this.BytesToQuaternion(data);
            else if ((typeof(T)) == typeof(Color))      result = this.BytesToColor(data);

            return result;
        }

        /// <summary>
        /// Converts a byte array to an object of a specified type.
        /// </summary>
        /// <typeparam name="E">The type of the object to convert to.</typeparam>
        /// <param name="data">The byte array to convert.</param>
        /// <returns>An object of type E represented by the byte array.</returns>
        private E FromByteArray<E>(byte[] data) {
            object result = default(object);
            if (data == null) {
                return default(E);
            }            
                 if ((typeof(E)) == typeof(int))        result = BitConverter.ToInt32(data);
            else if ((typeof(E)) == typeof(uint))       result = BitConverter.ToUInt32(data);
            else if ((typeof(E)) == typeof(ushort))     result = BitConverter.ToUInt16(data);
            else if ((typeof(E)) == typeof(float))      result = BitConverter.ToSingle(data);
            else if ((typeof(E)) == typeof(long))       result = BitConverter.ToInt64(data);
            else if ((typeof(E)) == typeof(ulong))      result = BitConverter.ToUInt64(data);
            else if ((typeof(E)) == typeof(short))      result = BitConverter.ToInt16(data);
            else if ((typeof(E)) == typeof(double))     result = BitConverter.ToDouble(data);
            else if ((typeof(E)) == typeof(char))       result = BitConverter.ToChar(data);
            else if ((typeof(E)) == typeof(bool))       result = BitConverter.ToBoolean(data);
            else if ((typeof(E)) == typeof(string))     result = Encoding.ASCII.GetString(data);
            else if ((typeof(E)) == typeof(sbyte))      result = (sbyte)data[0];
            else if ((typeof(E)) == typeof(byte))       result = data[0];
            else if ((typeof(E)) == typeof(byte[]))     result = data;
            else if ((typeof(E)) == typeof(Vector2))    result = this.BytesToVector2(data);
            else if ((typeof(E)) == typeof(Vector3))    result = this.BytesToVector3(data);
            else if ((typeof(E)) == typeof(Quaternion)) result = this.BytesToQuaternion(data);
            else if ((typeof(E)) == typeof(Color))      result = this.BytesToColor(data);

            return (E)result;
        }

        /// <summary>
        /// Converts a byte array to an object of a specified type.
        /// </summary>
        /// <typeparam name="E">The type of the object to convert to.</typeparam>
        /// <param name="data">The byte array to convert.</param>
        /// <param name="dataType">The type to convert the byte array to.</param>
        /// <returns>An object of type E represented by the byte array.</returns>
        private E FromByteArray<E>(byte[] data, Type dataType) {
            object result = default(object);
            if (data == null) {
                return default(E);
            }            
                 if (dataType == typeof(int))        result = BitConverter.ToInt32(data);
            else if (dataType == typeof(uint))       result = BitConverter.ToUInt32(data);
            else if (dataType == typeof(ushort))     result = BitConverter.ToUInt16(data);
            else if (dataType == typeof(float))      result = BitConverter.ToSingle(data);
            else if (dataType == typeof(long))       result = BitConverter.ToInt64(data);
            else if (dataType == typeof(ulong))      result = BitConverter.ToUInt64(data);
            else if (dataType == typeof(short))      result = BitConverter.ToInt16(data);
            else if (dataType == typeof(double))     result = BitConverter.ToDouble(data);
            else if (dataType == typeof(char))       result = BitConverter.ToChar(data);
            else if (dataType == typeof(bool))       result = BitConverter.ToBoolean(data);
            else if (dataType == typeof(string))     result = Encoding.ASCII.GetString(data);
            else if (dataType == typeof(sbyte))      result = (sbyte)data[0];
            else if (dataType == typeof(byte))       result = data[0];
            else if (dataType == typeof(byte[]))     result = data;
            else if (dataType == typeof(Vector2))    result = this.BytesToVector2(data);
            else if (dataType == typeof(Vector3))    result = this.BytesToVector3(data);
            else if (dataType == typeof(Quaternion)) result = this.BytesToQuaternion(data);
            else if (dataType == typeof(Color))      result = this.BytesToColor(data);

            return (E)result;
        }

        /// <summary>
        /// Convert a Vector2 to a byte array
        /// </summary>
        /// <param name="data">Vector2</param>
        /// <returns>Byte array representation of Vector2</returns>
        private byte[] Vector2ToBytes(Vector2 data) {
            byte[] bytes = new byte[8]; // 4 bytes per float

            Buffer.BlockCopy(BitConverter.GetBytes(data.x), 0, bytes, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(data.y), 0, bytes, 4, 4);            

            return bytes;
        }

        /// <summary>
        /// Convert a byte array to a Vector2
        /// </summary>
        /// <param name="data">Byte array</param>
        /// <returns>Insatnce of Vector2</returns>
        private Vector2 BytesToVector2(byte[] data) {
            Vector2 result = Vector2.zero;
            byte[] bytes = new byte[4]; // 4 bytes per float

            Buffer.BlockCopy(data, 0, bytes, 0, 4);
            result.x = BitConverter.ToSingle(bytes);

            Buffer.BlockCopy(data, 4, bytes, 0, 4);
            result.y = BitConverter.ToSingle(bytes);

            return result;
        }

        /// <summary>
        /// Convert a Vector3 to a byte array
        /// </summary>
        /// <param name="data">Vector3</param>
        /// <returns>Byte array representation of Vector3</returns>
        private byte[] Vector3ToBytes(Vector3 data) {
            byte[] bytes = new byte[12]; // 4 bytes per float

            Buffer.BlockCopy(BitConverter.GetBytes(data.x), 0, bytes, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(data.y), 0, bytes, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(data.z), 0, bytes, 8, 4);

            return bytes;
        }

        /// <summary>
        /// Convert a byte array to a Vector3
        /// </summary>
        /// <param name="data">Byte array</param>
        /// <returns>Instance of Vector3</returns>
        private Vector3 BytesToVector3(byte[] data) {
            Vector3 result = Vector3.zero;
            byte[] bytes = new byte[4]; // 4 bytes per float

            Buffer.BlockCopy(data, 0, bytes, 0, 4);
            result.x = BitConverter.ToSingle(bytes);

            Buffer.BlockCopy(data, 4, bytes, 0, 4);
            result.y = BitConverter.ToSingle(bytes);

            Buffer.BlockCopy(data, 8, bytes, 0, 4);
            result.z = BitConverter.ToSingle(bytes);

            return result;
        }

        /// <summary>
        /// Convert a Quaternion to a byte array
        /// </summary>
        /// <param name="data">Quaternion</param>
        /// <returns>Byte array representation of Quaternion</returns>
        private byte[] QuaternionToBytes(Quaternion data) {
            byte[] bytes = new byte[16]; // 4 bytes per float

            Buffer.BlockCopy(BitConverter.GetBytes(data.x), 0, bytes, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(data.y), 0, bytes, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(data.z), 0, bytes, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(data.w), 0, bytes, 8, 4);

            return bytes;
        }

        /// <summary>
        /// Convert a byte array to a Quaternion
        /// </summary>
        /// <param name="data">Byte array</param>
        /// <returns>Instance of Quaternion</returns>
        private Quaternion BytesToQuaternion(byte[] data) {
            Quaternion result = Quaternion.identity;
            byte[] bytes = new byte[4]; // 4 bytes per float

            Buffer.BlockCopy(data, 0, bytes, 0, 4);
            result.x = BitConverter.ToSingle(bytes);

            Buffer.BlockCopy(data, 4, bytes, 0, 4);
            result.y = BitConverter.ToSingle(bytes);

            Buffer.BlockCopy(data, 8, bytes, 0, 4);
            result.z = BitConverter.ToSingle(bytes);

            Buffer.BlockCopy(data, 12, bytes, 0, 4);
            result.w = BitConverter.ToSingle(bytes);

            return result;
        }

        /// <summary>
        /// Convert Color to a byte array
        /// </summary>
        /// <param name="data">Byte array</param>
        /// <returns>Byte array representation of Color</returns>
        private byte[] ColorToBytes(Color data)
        {
            byte r = (byte)(data.r * 255);
            byte g = (byte)(data.g * 255);
            byte b = (byte)(data.b * 255);
            byte a = (byte)(data.a * 255);

            // Combine into a byte array
            return new byte[] { r, g, b, a };
        }

        /// <summary>
        /// Convert a byte array to a Color
        /// </summary>
        /// <param name="bytes">Byte array</param>
        /// <returns>Instance of Color</returns>
        private Color BytesToColor(byte[] bytes)
        {
            // Normalize to 0–1 range
            return new Color(bytes[0] / 255f, bytes[1] / 255f, bytes[2] / 255f, bytes[3] / 255f);
        }
    }
}