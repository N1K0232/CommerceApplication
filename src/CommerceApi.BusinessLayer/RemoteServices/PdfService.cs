using CommerceApi.BusinessLayer.RemoteServices.Interfaces;
using CommerceApi.DataAccessLayer.Entities;
using CommerceApi.StorageProviders;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CommerceApi.BusinessLayer.RemoteServices;

public class PdfService : IPdfService
{
    private readonly IStorageProvider storageProvider;

    public PdfService(IStorageProvider storageProvider)
    {
        this.storageProvider = storageProvider;
    }

    public async Task UploadPdfInvoiceAsync(Invoice invoice)
    {
        var pdfStream = new MemoryStream();
        var document = new Document(PageSize.A4);
        var writer = PdfWriter.GetInstance(document, pdfStream);

        document.Open();

        var table = new PdfPTable(5);
        var phrase = new Phrase("Invoices");

        var cell = new PdfPCell(phrase)
        {
            Colspan = 2,
            HorizontalAlignment = 1
        };

        table.AddCell(cell);
        table.AddCell(invoice.Product.Name);
        table.AddCell(invoice.Price.ToString());
        table.AddCell(invoice.Quantity.ToString());
        table.AddCell(invoice.TotalPrice.ToString());

        document.Add(table);

        document.Close();
        writer.Close();

        pdfStream.Position = 0;
        await storageProvider.SaveAsync(invoice.DownloadFileName, pdfStream);
    }
}