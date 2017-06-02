﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabelGenerator;
using FluentAssertions;
using System.IO;
using LabelGenerator.Settings;

namespace LabelGeneratorTests
{
    [TestClass]
    public class GeneratePdfTests
    {
        string[][] testAddresses;

        [TestInitialize]
        public void Init()
        {
            var addressList = new List<string[]>();
            for (int i = 0; i < 30; i++)
            {
                var address = new string[] { "TolvanTolvanTolvanTolvanTolvanTolvanTolvanTolvanTolvanTo TolvanssonTolvanTolvanssonTolvanTolvanssonTolvanTo", "c/o Elvan Elvansson", "Tolvvägen 12", "12345 Tolvstad", "Sverige" };
                addressList.Add(address);
                address = new string[] { "Anita Andersson", "c/o Ante Andersson", "Betavägen 2", "12345 Saltö", "Sverige" };
                addressList.Add(address);
                address = new string[] { "Bertil Cederqvist", "c/o Berit Cederqvist", "Djurövägen 2", "12345 Djurö", "Sverige" };
                addressList.Add(address);
            }
            testAddresses = addressList.ToArray();
        }


        [TestMethod]
        public void TestSettingsAreGeneratedForSpecificDocumentType()
        {
            var docType = DocumentType.A4_2Columns8Rows;
            var settings = CreatePdf.GetSettings(docType);
            settings.Should().BeOfType<LabelSettings_A4_2Columns8Rows>();

            docType = DocumentType.A4_3Columns8Rows;
            settings = CreatePdf.GetSettings(docType);
            settings.Should().BeOfType<LabelSettings_A4_3Columns8Rows>();
        }

        [TestMethod]
        public void TestNewPageWhenMaximumLabelCountIsReached()
        {
            testAddresses = testAddresses.Concat(testAddresses).ToArray();

            var documentType = DocumentType.A4_2Columns8Rows;
            var result = CreatePdf.CreateDocument(testAddresses, documentType);
            result.PageCount.Should().BeGreaterThan(1);
        }

        [TestMethod]
        public void TestRectangleReturnsValue()
        {
            var settings = new LabelSettings_A4_2Columns8Rows();
            var contentSize = CreatePdf.GetContentSize(settings);
            var result = CreatePdf.CreateRectangle(settings.LabelPositionX, settings.LabelPositionY, contentSize);
            result.IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void TestAddPageHeightAndWidthShouldBeConvertedToTypePoint()
        {
            var settings = new LabelSettings_A4_2Columns8Rows();
            var document = new PdfSharp.Pdf.PdfDocument();
            var result = CreatePdf.AddPage(document, settings);

            result.Width.Type.Should().Be(PdfSharp.Drawing.XGraphicsUnit.Point);
            result.Height.Type.Should().Be(PdfSharp.Drawing.XGraphicsUnit.Point);
        }

        [TestMethod]
        public void TestAddressesAreFormattedCorrectly()
        {
            string[] address = new string[] { "Tolvan Tolvansson", "c/o Elvan Elvansson", "Tolvgatan 12", "12345 Tolvstad", "Sverige" };
            var result = CreatePdf.FormatLabelText(address, 60);
            result.Should().Be("Tolvan Tolvansson\r\nc/o Elvan Elvansson\r\nTolvgatan 12\r\n12345 Tolvstad\r\nSverige\r\n");
        }

        [TestMethod]
        public void TestLongLabelTextIsTruncated()
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var randomText = new string(Enumerable.Repeat(chars, 80).Select(s => s[random.Next(s.Length)]).ToArray());
            string[] address = new string[] { $"Tolvan {randomText}" };
            
            var settings = new LabelSettings_A4_2Columns8Rows();
            var totalMaxLength = Environment.NewLine.Length + settings.MaxCharactersPerRow;

            var result = CreatePdf.FormatLabelText(address, settings.MaxCharactersPerRow);
            result.Length.Should().Be(totalMaxLength);
        }

        [TestMethod]
        public void TestColumnCalculation()
        {
            //---- 2 COLUMNS PER PAGE

            var columnsPerPage = 2;
            var labelsInPage = 0;
            var result = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            result.Should().Be(1); // First column

            columnsPerPage = 2;
            labelsInPage = 1;
            result = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            result.Should().Be(2); // Second column

            columnsPerPage = 2;
            labelsInPage = 2;
            result = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            result.Should().Be(1); // First column

            //---- 3 COLUMNS PER PAGE

            columnsPerPage = 3;
            labelsInPage = 0;
            result = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            result.Should().Be(1); // First column

            columnsPerPage = 3;
            labelsInPage = 1;
            result = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            result.Should().Be(2); // Second column

            columnsPerPage = 3;
            labelsInPage = 2;
            result = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            result.Should().Be(3); // Third column

            columnsPerPage = 3;
            labelsInPage = 3;
            result = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            result.Should().Be(1); // First column

            //---- 4 COLUMNS PER PAGE

            columnsPerPage = 4;
            labelsInPage = 0;
            result = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            result.Should().Be(1); // First column

            columnsPerPage = 4;
            labelsInPage = 1;
            result = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            result.Should().Be(2); // Second column

            columnsPerPage = 4;
            labelsInPage = 2;
            result = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            result.Should().Be(3); // Third column

            columnsPerPage = 4;
            labelsInPage = 3;
            result = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            result.Should().Be(4); // Forth column

            columnsPerPage = 4;
            labelsInPage = 4;
            result = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            result.Should().Be(1); // First column
        }

        [TestMethod]
        public void TestRowCalculation()
        {
            //---- 2 COLUMNS PER PAGE

            var columnsPerPage = 2;
            var labelsInPage = 0; // No labels added yet
            int currentRow;
            var currentColumn = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            currentRow = CreatePdf.CalculateCurrentRow(labelsInPage, columnsPerPage, currentColumn);
            currentRow.Should().Be(1);

            labelsInPage++;
            currentColumn = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            currentRow = CreatePdf.CalculateCurrentRow(labelsInPage, columnsPerPage, currentColumn);
            currentRow.Should().Be(1);

            // New row
            labelsInPage++;
            currentColumn = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            currentRow = CreatePdf.CalculateCurrentRow(labelsInPage, columnsPerPage, currentColumn);
            currentRow.Should().Be(2);

            labelsInPage++;
            currentColumn = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            currentRow = CreatePdf.CalculateCurrentRow(labelsInPage, columnsPerPage, currentColumn);
            currentRow.Should().Be(2);

            // New row
            labelsInPage++;
            currentColumn = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            currentRow = CreatePdf.CalculateCurrentRow(labelsInPage, columnsPerPage, currentColumn);
            currentRow.Should().Be(3);

            //---- 3 COLUMNS PER PAGE

            columnsPerPage = 3;
            labelsInPage = 0; // No labels added yet
            currentRow = 1; // Start value
            currentColumn = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            currentRow = CreatePdf.CalculateCurrentRow(labelsInPage, columnsPerPage, currentColumn);
            currentRow.Should().Be(1);

            labelsInPage++;
            currentColumn = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            currentRow = CreatePdf.CalculateCurrentRow(labelsInPage, columnsPerPage, currentColumn);
            currentRow.Should().Be(1);


            labelsInPage++;
            currentColumn = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            currentRow = CreatePdf.CalculateCurrentRow(labelsInPage, columnsPerPage, currentColumn);
            currentRow.Should().Be(1);

            // New row
            labelsInPage++;
            currentColumn = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            currentRow = CreatePdf.CalculateCurrentRow(labelsInPage, columnsPerPage, currentColumn);
            currentRow.Should().Be(2);

            //----  4 COLUMNS PER PAGE

            columnsPerPage = 4;
            labelsInPage = 0; // No labels added yet
            currentRow = 1; // Start value
            currentColumn = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            currentRow = CreatePdf.CalculateCurrentRow(labelsInPage, columnsPerPage, currentColumn);
            currentRow.Should().Be(1);

            labelsInPage = 3;
            currentColumn = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            currentRow = CreatePdf.CalculateCurrentRow(labelsInPage, columnsPerPage, currentColumn);
            currentRow.Should().Be(1);

            // New row
            labelsInPage = 4;
            currentColumn = CreatePdf.CalculateCurrentColumn(labelsInPage, columnsPerPage);
            currentRow = CreatePdf.CalculateCurrentRow(labelsInPage, columnsPerPage, currentColumn);
            currentRow.Should().Be(2);
        }

        [TestMethod]
        public void TestContentPositionCalculation()
        {
            var settings = new LabelSettings_A4_2Columns8Rows();
            var currentColumn = 1;

            var result = CreatePdf.CalculateContentPositionLeft(currentColumn, settings);
            result.Should().Be(settings.LabelPaddingLeft + settings.LabelMarginLeft);

            currentColumn = 2;
            result = CreatePdf.CalculateContentPositionLeft(currentColumn, settings);
            result.Should().Be(settings.LabelPaddingLeft + settings.LabelMarginLeft + settings.LabelPositionX);

            var currentRow = 1;
            result = CreatePdf.CalculateContentPositionTop(currentRow, settings);
            result.Should().Be(settings.LabelPaddingTop + settings.LabelMarginTop);

            currentRow = 2;
            result = CreatePdf.CalculateContentPositionTop(currentRow, settings);
            result.Should().Be(settings.LabelPaddingTop + settings.LabelMarginTop + settings.LabelPositionY);
        }

        // TEMP HACK FOR WRITING PDF TO DISC, TO BE REMOVED
        [TestMethod]
        public void TestWriteDocument2ColumnsToDisc()
        {
            var documentType = DocumentType.A4_2Columns8Rows;
            var document = CreatePdf.CreateDocument(testAddresses, documentType);
            var documentAsByteArray = CreatePdf.SaveToArray(document);
            File.WriteAllBytes(@"C:\Temp\TestFile_2Columns.pdf", documentAsByteArray);
        }
        // TEMP HACK FOR WRITING PDF TO DISC, TO BE REMOVED
        [TestMethod]
        public void TestWriteDocument3ColumnsToDisc()
        {
            var documentType = DocumentType.A4_3Columns8Rows;
            var document = CreatePdf.CreateDocument(testAddresses, documentType);
            var documentAsByteArray = CreatePdf.SaveToArray(document);
            File.WriteAllBytes(@"C:\Temp\TestFile_3Columns.pdf", documentAsByteArray);
        }
    }
}
