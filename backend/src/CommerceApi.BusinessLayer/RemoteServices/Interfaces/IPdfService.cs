using iTextSharp.text.pdf;

namespace CommerceApi.BusinessLayer.RemoteServices.Interfaces;

public interface IPdfService
{
    Task UploadAsync(string path, string phraseName, params PdfPCell[] cells);
}