﻿using CommerceApi.BusinessLayer.RemoteServices.Interfaces;
using CommerceApi.StorageProviders.Abstractions;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CommerceApi.BusinessLayer.RemoteServices;

public class PdfService : IPdfService
{
    private readonly IStorageProvider _storageProvider;

    public PdfService(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public async Task UploadAsync(string path, string phraseName, params PdfPCell[] cells)
    {
        var pdfStream = new MemoryStream();
        var document = new Document(PageSize.A4);

        var writer = PdfWriter.GetInstance(document, pdfStream);
        document.Open();

        var table = new PdfPTable(cells.Length + 1);
        var phrase = new Phrase(phraseName);

        var phraseCell = new PdfPCell(phrase) { Colspan = 2, HorizontalAlignment = 1 };
        table.AddCell(phraseCell);

        foreach (var cell in cells)
        {
            table.AddCell(cell);
        }

        document.Add(table);
        document.Close();

        writer.Close();
        pdfStream.Position = 0;

        await _storageProvider.SaveAsync(path, pdfStream, true);
        await pdfStream.DisposeAsync();
    }
}