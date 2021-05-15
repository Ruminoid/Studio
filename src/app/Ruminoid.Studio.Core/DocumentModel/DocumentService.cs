using System;
using System.IO;
using System.Xml.Serialization;

namespace Ruminoid.Studio.DocumentModel
{
    public interface IDocumentService
    {
        #region 文件操作

        /// <summary>
        /// 打开文档。
        /// </summary>
        /// <param name="documentPath">文档的位置。</param>
        /// <remarks>
        /// <para>
        /// 使用 <paramref name="documentPath"/> 读取文件并打开文档。
        /// </para>
        /// <para>
        /// 注意：永远不应该直接使用这个方法。请使用封装的命令。
        /// </para>
        /// </remarks>
        public void OpenDocument(string documentPath);

        /// <summary>
        /// 关闭文档。
        /// </summary>
        /// <remarks>
        /// <para>
        /// 直接关闭文档。
        /// </para>
        /// <para>
        /// 注意：永远不应该直接使用这个方法。请使用封装的命令。
        /// </para>
        /// </remarks>
        public void CloseDocument();

        /// <summary>
        /// 保存文档。
        /// </summary>
        /// <remarks>
        /// <para>
        /// 保存文档到 <see cref="DocumentPath"/>。
        /// </para>
        /// <para>
        /// 注意：永远不应该直接使用这个方法。请使用封装的命令。
        /// </para>
        /// </remarks>
        public void SaveDocument();

        #endregion

        #region 文档

        /// <summary>
        /// 文档（.rmproj）。
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="Document"/> ——文档（.rmproj）是 Ruminoid Studio 的总文档模型，
        /// 由 <see cref="DocumentService"/> 负责管理。
        /// </para>
        /// <para>
        /// 任何模块均应该设置对 <see cref="Document"/> 的直接绑定。
        /// 切勿设置对其字段或属性的子绑定。
        /// </para>
        /// <para>
        /// 在各模块的生命周期内，Ruminoid Studio 可以保证
        /// <see cref="DocumentService"/> 及 <see cref="Document"/>
        /// 实例均不会改变。
        /// </para>
        /// <para>
        /// 不保证 <see cref="Document"/> 的成员不会发生改变。
        /// </para>
        /// </remarks>
        public Document Document { get; }

        /// <summary>
        /// 文档的位置。
        /// </summary>
        public string DocumentPath { get; }

        #endregion
    }

    public sealed class DocumentService : IDocumentService
    {
        #region 文件操作

        public void OpenDocument(string documentPath)
        {
            // 加载文档
            using FileStream fileStream = File.OpenRead(documentPath);

            // 反序列化
            XmlSerializer serializer = new(typeof(Document));
            Document = serializer.Deserialize(fileStream) as Document;

            if (Document is null)
                throw new DocumentServiceException("文档加载出现问题。");

            // 设置文档位置
            DocumentPath = documentPath;
        }

        public void CloseDocument()
        {
            Document = null;
            DocumentPath = string.Empty;
        }

        public void SaveDocument()
        {
            // 加载文档
            using FileStream fileStream = File.Open(DocumentPath, FileMode.Create);

            // 序列化
            XmlSerializer serializer = new(typeof(Document));
            serializer.Serialize(fileStream, Document);
        }

        #endregion

        #region 文档

        public Document Document { get; private set; }

        public string DocumentPath { get; private set; }

        #endregion
    }

    [Serializable]
    public class DocumentServiceException : Exception
    {
        public DocumentServiceException()
        {
        }

        public DocumentServiceException(string message) : base(message)
        {
        }

        public DocumentServiceException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
