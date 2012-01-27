Imports System.Xml
Imports System
Imports System.Data
Imports System.Web.Security
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls
Imports Ektron.Cms
Imports Ektron.Cms.API
Imports Ektron.Cms.Common

Imports Ektron.Cms.Commerce


'**************************   STAGING SERVER ********************************



Partial Class admin
    Inherits System.Web.UI.Page
    'ToDo: put these values in web.config
    'Const FOLDER_ID As Integer = 30
    'Const TAXONOMY_ID As Integer = 4 'Ignitions
    Const CONTENT_LANGUAGE As Integer = 1033
    Const GO_LIVE As String = ""
    Const END_DATE As String = ""
    Const FILE_PATH As String = "data\"
    Const META_OBDIILEGAL As String = "128"
    Const META_SHOPATRONBUYLINK As String = "130"
    Const META_REPLACEMENTPARTS As String = "120"
    Const META_REQUIREDCOMPONENTS As String = "132"
    Const META_ACCESSORIES As String = "143"
    Const LOG_PATH As String = "data\"
    'Const UPLOADED_FILES_PATH As String = "/uploadedfiles/MSDIgnitioncom/Products/Acceories/"
    'Const UPLOADED_IMAGES_PATH As String = "/uploadedimages/MSDIgnitioncom/Products/Accesories/"
    Const HREF_TARGET_BLANK As String = "_blank"
    'Development variables
    'Const META_OBDIILEGAL As String = "129"
    'Const META_SHOPATRONBUYLINK As String = "131"
    'Const UPLOADED_FILES_PATH As String = "/MSDCMSv1/uploadedfiles/MSDIgnitioncom/Products/Ignitions/"
    'Const UPLOADED_IMAGES_PATH As String = "/MSDCMSv1/uploadedimages/MSDIgnitioncom/Products/Ignitions/"


    Protected Sub btnInstructions_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnInstructions.Click
        ReplaceInstructions("RPM Timing Controls", "instructions01182012")
    End Sub

    Private Function getProductNumber(ByVal productTitle As String) As String
        Dim arr As String() = productTitle.Split("-")
        Return (arr(0).Trim())
    End Function

    Private Sub ReplaceInstructions(ByVal productFolder As String, ByVal instructionsFolder as String)
        Dim contentAPI As New Ektron.Cms.Framework.Core.Content.Content()
        Dim oXMLDoc As New XmlDocument
        Dim oXMLNodeList As XmlNodeList
        Dim oXMLNode As XmlNode
        Dim root As XmlNode
        Dim messLog As New MessageLogger("/inetpub/MSDIgnitionV8_Staging/logs/instructionslog.txt")

        Dim criteriaInstructions As New Ektron.Cms.Common.Criteria(Of ContentProperty)
        Dim listInstructions As System.Collections.Generic.List(Of ContentData)
        Dim instructionData As ContentData

        Dim criteriaProducts As New Ektron.Cms.Common.Criteria(Of ContentProperty)
        Dim listProducts As New System.Collections.Generic.List(Of ContentData)
        Dim productData As ContentData

        'criteriaProducts.AddFilter(ContentProperty.Title, CriteriaFilterOperator.StartsWith, "8963 ")
        'criteriaProducts.AddFilter(ContentProperty.FolderName, CriteriaFilterOperator.EqualTo, "RPM Timing Controls")

        criteriaProducts.AddFilter(ContentProperty.FolderName, CriteriaFilterOperator.EqualTo, productFolder)
        listProducts = contentAPI.GetList(criteriaProducts)
        Try
            For Each productData In listProducts

                oXMLDoc.LoadXml(productData.Html)
                root = oXMLDoc.DocumentElement

                criteriaInstructions.Filters.Clear()
                criteriaInstructions.AddFilter(ContentProperty.Title, CriteriaFilterOperator.StartsWith, getProductNumber(productData.Title))
                criteriaInstructions.AddFilter(ContentProperty.FolderName, CriteriaFilterOperator.EqualTo, instructionsFolder)

                listInstructions = contentAPI.GetList(criteriaInstructions)

                'Delete previous nodes only if we find new ones
                If listInstructions.Count > 0 Then
                    oXMLNodeList = root.SelectNodes("//instructions")
                    For Each oXMLNode In oXMLNodeList
                        oXMLNode.ParentNode.RemoveChild(oXMLNode)
                    Next

                    For Each instructionData In listInstructions
                        Dim newInstructions As XmlNode
                        Dim anchor As XmlNode
                        Dim aHref As XmlAttribute
                        Dim aTarget As XmlAttribute
                        Dim aTitle As XmlAttribute

                        newInstructions = oXMLDoc.CreateElement("instructions")
                        aHref = oXMLDoc.CreateAttribute("href")
                        aTarget = oXMLDoc.CreateAttribute("target")
                        aTitle = oXMLDoc.CreateAttribute("title")
                        anchor = oXMLDoc.CreateElement("a")

                        aHref.Value = instructionData.Quicklink
                        aTarget.Value = "_blank"
                        aTitle.Value = instructionData.Title

                        anchor.Attributes.Append(aHref)
                        anchor.Attributes.Append(aTarget)
                        anchor.Attributes.Append(aTitle)
                        anchor.InnerText = instructionData.Title

                        newInstructions.AppendChild(anchor)
                        root("product").AppendChild(newInstructions)
						messLog.LogMessage(productData.Title & " -> " & instructionData.Title & Environment.NewLine)
                    Next

                    productData.Html = root.OuterXml
                    contentAPI.Update(productData)
                    

                End If
            Next
        Catch e As Exception
            messLog.LogMessage(e.Message)
        End Try


    End Sub
    Private Function AddContentIDtoTaxonomy(ByVal contentid As Long, ByVal taxonomyid As Long) As Boolean
        Dim apiTax As New Ektron.Cms.API.Content.Taxonomy()

        Dim item_request As New Ektron.Cms.TaxonomyRequest
        item_request.TaxonomyId = taxonomyid
        item_request.TaxonomyIdList = contentid
        item_request.TaxonomyLanguage = CONTENT_LANGUAGE
        apiTax.AddTaxonomyItem(item_request)

        Return True
    End Function
    'Build the XML text to upload to the SmartForm in the CMS
    'Input: XML structured data
    'Output: XML data ready to be used with CMS
    Private Function BuildProductInfoXML(ByVal oProduct As MSDProduct) As String
        Dim oXMLDoc As New XmlDocument
        Dim oXMLRoot As XmlNode
        Dim oXMLProduct As XmlNode
        Dim oXMLElement As XmlNode
        Dim oXMLElementSubLevel2 As XmlNode
        Dim oXMLAttribute As XmlNode
        Dim oXMLWhite As XmlWhitespace

        oXMLRoot = oXMLDoc.CreateElement("root")
        oXMLProduct = oXMLDoc.CreateElement("product")

        oXMLDoc.AppendChild(oXMLRoot)
        oXMLRoot.AppendChild(oXMLProduct)

        With oXMLProduct

            oXMLElement = oXMLDoc.CreateElement("partnumber")
            oXMLElement.InnerText = oProduct.PartNumber
            .AppendChild(oXMLElement)
            oXMLWhite = oXMLDoc.CreateWhitespace(ControlChars.CrLf)
            .InsertBefore(oXMLWhite, oXMLElement)

            oXMLElement = oXMLDoc.CreateElement("productname")
            oXMLElement.InnerText = oProduct.Name
            .AppendChild(oXMLElement)
            oXMLWhite = oXMLDoc.CreateWhitespace(ControlChars.CrLf)
            .InsertBefore(oXMLWhite, oXMLElement)

            oXMLElement = oXMLDoc.CreateElement("description")
            oXMLElement.InnerXml = oProduct.Description
            .AppendChild(oXMLElement)
            oXMLWhite = oXMLDoc.CreateWhitespace(ControlChars.CrLf)
            .InsertBefore(oXMLWhite, oXMLElement)

            oXMLElement = oXMLDoc.CreateElement("shortdescription")
            oXMLElement.InnerXml = oProduct.Description
            .AppendChild(oXMLElement)
            oXMLWhite = oXMLDoc.CreateWhitespace(ControlChars.CrLf)
            .InsertBefore(oXMLWhite, oXMLElement)

            oXMLElement = oXMLDoc.CreateElement("bulletsattributes")
            oXMLElement.InnerXml = oProduct.BulletAttributes
            .AppendChild(oXMLElement)
            oXMLWhite = oXMLDoc.CreateWhitespace(ControlChars.CrLf)
            .InsertBefore(oXMLWhite, oXMLElement)

            oXMLElement = oXMLDoc.CreateElement("specifications")
            oXMLElement.InnerXml = oProduct.Specifications
            .AppendChild(oXMLElement)
            oXMLWhite = oXMLDoc.CreateWhitespace(ControlChars.CrLf)
            .InsertBefore(oXMLWhite, oXMLElement)

            oXMLElement = oXMLDoc.CreateElement("techtips")
            oXMLElement.InnerXml = oProduct.TechTips
            .AppendChild(oXMLElement)
            oXMLWhite = oXMLDoc.CreateWhitespace(ControlChars.CrLf)
            .InsertBefore(oXMLWhite, oXMLElement)

            oXMLElement = oXMLDoc.CreateElement("mvpprice")
            oXMLElement.InnerText = oProduct.MVPPrice
            .AppendChild(oXMLElement)
            oXMLWhite = oXMLDoc.CreateWhitespace(ControlChars.CrLf)
            .InsertBefore(oXMLWhite, oXMLElement)

            'Add graphic tree
            oXMLElement = oXMLDoc.CreateElement("graphic")
            oXMLElementSubLevel2 = oXMLDoc.CreateElement("img")
            oXMLAttribute = oXMLDoc.CreateAttribute("alt")
            oXMLAttribute.Value = oProduct.GraphicAltText
            oXMLElementSubLevel2.Attributes.Append(oXMLAttribute)
            oXMLAttribute = oXMLDoc.CreateAttribute("src")
            oXMLAttribute.Value = oProduct.UploadedImagesPath & oProduct.Image
            oXMLElementSubLevel2.Attributes.Append(oXMLAttribute)
            oXMLElement.AppendChild(oXMLElementSubLevel2)
            .AppendChild(oXMLElement)
            oXMLWhite = oXMLDoc.CreateWhitespace(ControlChars.CrLf)
            .InsertBefore(oXMLWhite, oXMLElement)
            '''''''''''''''''''
            'Add productimage tree
            oXMLElement = oXMLDoc.CreateElement("productimage")
            oXMLElementSubLevel2 = oXMLDoc.CreateElement("img")
            oXMLAttribute = oXMLDoc.CreateAttribute("alt")
            oXMLAttribute.Value = oProduct.ImageAltText
            oXMLElementSubLevel2.Attributes.Append(oXMLAttribute)
            oXMLAttribute = oXMLDoc.CreateAttribute("src")
            oXMLAttribute.Value = oProduct.UploadedImagesPath & oProduct.Image
            oXMLElementSubLevel2.Attributes.Append(oXMLAttribute)
            oXMLElement.AppendChild(oXMLElementSubLevel2)
            .AppendChild(oXMLElement)
            oXMLWhite = oXMLDoc.CreateWhitespace(ControlChars.CrLf)
            .InsertBefore(oXMLWhite, oXMLElement)
            '''''''''''''''''''
            'Add instructions tree
            If Not oProduct.Instructions = String.empty Then
                oXMLElement = oXMLDoc.CreateElement("instructions")
                oXMLElementSubLevel2 = oXMLDoc.CreateElement("a")
                oXMLElementSubLevel2.InnerText = oProduct.Instructions
                oXMLAttribute = oXMLDoc.CreateAttribute("href")
                oXMLAttribute.Value = oProduct.UploadedFilesPath & oProduct.InstructionsHref
                oXMLElementSubLevel2.Attributes.Append(oXMLAttribute)
                oXMLAttribute = oXMLDoc.CreateAttribute("title")
                oXMLAttribute.Value = oProduct.InstructionsTitle
                oXMLElementSubLevel2.Attributes.Append(oXMLAttribute)
                oXMLAttribute = oXMLDoc.CreateAttribute("target")
                oXMLAttribute.Value = HREF_TARGET_BLANK
                oXMLElementSubLevel2.Attributes.Append(oXMLAttribute)
                oXMLElement.AppendChild(oXMLElementSubLevel2)
                .AppendChild(oXMLElement)
                oXMLWhite = oXMLDoc.CreateWhitespace(ControlChars.CrLf)
                .InsertBefore(oXMLWhite, oXMLElement)
            End If
            '''''''''''''''''''
            oXMLElement = oXMLDoc.CreateElement("carbapproved")
            oXMLElement.InnerText = oProduct.IsCarbApproved
            .AppendChild(oXMLElement)
            oXMLWhite = oXMLDoc.CreateWhitespace(ControlChars.CrLf)
            .InsertBefore(oXMLWhite, oXMLElement)
        End With

        Return (oXMLDoc.InnerXml)
    End Function
    'Build the XML text for MetaData 
    'Input: Semicolon separated ContentIDs (i.e. "32;34")
    'Output: XML document for use with CMS
    'ToDo: Input array to handle multiple metadata ids
    Private Function BuildMetaDataXML(ByVal oProduct As MSDProduct) As String
        Dim oXMLDoc As New XmlDocument
        Dim oXMLRoot As XmlNode
        Dim oXMLMetaElement As XmlNode
        Dim oXMLMetaAttribute As XmlNode

        oXMLRoot = oXMLDoc.CreateElement("metadata")
        oXMLDoc.AppendChild(oXMLRoot)

        oXMLMetaAttribute = oXMLDoc.CreateAttribute("id")
        oXMLMetaAttribute.Value = META_OBDIILEGAL
        oXMLMetaElement = oXMLDoc.CreateElement("meta")
        oXMLMetaElement.Attributes.Append(oXMLMetaAttribute)
        oXMLMetaElement.InnerText = oProduct.IsOBDIILegal
        oXMLRoot.AppendChild(oXMLMetaElement)

        oXMLMetaAttribute = oXMLDoc.CreateAttribute("id")
        oXMLMetaAttribute.Value = META_SHOPATRONBUYLINK
        oXMLMetaElement = oXMLDoc.CreateElement("meta")
        oXMLMetaElement.Attributes.Append(oXMLMetaAttribute)
        oXMLMetaElement.InnerText = oProduct.ShopatronBuyLink
        oXMLRoot.AppendChild(oXMLMetaElement)

        oXMLMetaAttribute = oXMLDoc.CreateAttribute("id")
        oXMLMetaAttribute.Value = META_REPLACEMENTPARTS
        oXMLMetaElement = oXMLDoc.CreateElement("meta")
        oXMLMetaElement.Attributes.Append(oXMLMetaAttribute)
        oXMLMetaElement.InnerText = oProduct.ReplacementParts
        oXMLRoot.AppendChild(oXMLMetaElement)

        oXMLMetaAttribute = oXMLDoc.CreateAttribute("id")
        oXMLMetaAttribute.Value = META_REQUIREDCOMPONENTS
        oXMLMetaElement = oXMLDoc.CreateElement("meta")
        oXMLMetaElement.Attributes.Append(oXMLMetaAttribute)
        oXMLMetaElement.InnerText = oProduct.RequiredComponents
        oXMLRoot.AppendChild(oXMLMetaElement)

        oXMLMetaAttribute = oXMLDoc.CreateAttribute("id")
        oXMLMetaAttribute.Value = META_ACCESSORIES
        oXMLMetaElement = oXMLDoc.CreateElement("meta")
        oXMLMetaElement.Attributes.Append(oXMLMetaAttribute)
        oXMLMetaElement.InnerText = oProduct.Accessories
        oXMLRoot.AppendChild(oXMLMetaElement)

        Return (oXMLDoc.OuterXml)

        'Dim sb As New StringBuilder
        ''ToDo: Do not use string builder, use XMLDoc
        'sb.Append("<metadata>")
        ''OBD
        'sb.Append("<meta id=""" & META_OBDIILEGAL & """>")
        'sb.Append(oProduct.IsOBDIILegal)
        'sb.Append("</meta>")
        ''Shopatron
        'sb.Append("<meta id=""" & META_SHOPATRONBUYLINK & """>")
        'sb.Append(oProduct.ShopatronBuyLink)
        'sb.Append("</meta>")
        'sb.Append("</metadata>")
        'Return (sb.ToString)
    End Function

    Private Function UploadFileToServer() As String
        Dim messLogger As New MessageLogger(HttpContext.Current.Server.MapPath(LOG_PATH))
        If FileUpload1.HasFile Then
            Dim filepath As String = HttpContext.Current.Server.MapPath(FILE_PATH)
            Try
                FileUpload1.SaveAs(filepath & _
                   FileUpload1.FileName)
                errorMessage.Text = "File name: " & _
                   FileUpload1.PostedFile.FileName & "<br>" & _
                   "File Size: " & _
                   FileUpload1.PostedFile.ContentLength & " kb<br>" & _
                   "Content type: " & _
                   FileUpload1.PostedFile.ContentType
            Catch ex As Exception
                messLogger.LogMessage("File upload error:" & ex.Message.ToString())
            End Try
            messLogger.LogMessage("Done with file upload!")
            Return (filepath)
        Else
            errorMessage.Text = "You have not specified a file."
            Return (String.Empty)
        End If
    End Function

    Protected Sub btnLoad_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnLoad.Click
        'Main program
        Dim filePath As String
        Dim messLogger As New MessageLogger(HttpContext.Current.Server.MapPath(LOG_PATH))

        messLogger.LogMessage("Start uploading file")
        filePath = UploadFileToServer()
        'ReadXML
        Dim xmldoc As New XmlDocument
        Dim ContentID As Long

        'First check if a file was uploaded
        If Not filePath = String.Empty Then
            Dim oXMLTextReader As New XmlTextReader(filePath & FileUpload1.FileName)
            Dim oProduct As New MSDProduct()
            Dim oXMLNode As XmlNode = xmldoc.CreateElement("product")

            'Check to see if the action is NEW or UPDATE
            If rdNew.Checked = True Then
                While oXMLTextReader.Read
                    If oXMLTextReader.Name = "product" Then
                        oXMLNode.InnerXml = oXMLTextReader.ReadInnerXml
                        oProduct.Clear()
                        Try
                            oProduct.LoadFromXML(oXMLNode)
                            ContentID = AddProduct(oProduct)
                        Catch ex As System.NullReferenceException
                            messLogger.LogMessage(ex.Message)
                        End Try

                        'Assign(taxonomy)
                        'ToDo: add error handling
                        If Not ContentID = 0 Then
                            AddContentIDtoTaxonomy(ContentID, oProduct.TaxonomyID)
                        End If
                    End If

                End While
            Else
                While oXMLTextReader.Read
                    If oXMLTextReader.Name = "product" Then
                        'oXMLNode.InnerXml = HttpUtility.HtmlEncode(oXMLTextReader.ReadInnerXml)
                        oXMLNode.InnerXml = oXMLTextReader.ReadInnerXml
                        oProduct.Clear()
                        Try
                            oProduct.LoadFromXML(oXMLNode)
                            'ToDo: Dynamically choose to update metadata or not
                            UpdateProduct(oProduct, False)
                            ContentID = oProduct.ContentID
                        Catch ex As Exception
                            messLogger.LogMessage(ex.Message & " - In LoadFromXML")
                        End Try
                    End If
                End While
            End If
            messLogger.LogMessage("Done procesing XML file.")
        End If
        messLogger.LogMessage("Done!")
        errorMessage.Text = "Done! Check log for possible error messages."

        'xmldoc.Load(filePath & FileUpload1.FileName)
        'xmlNodeList = xmldoc.SelectNodes("/products/product")

        ''First check if a file was uploaded
        'If Not filePath = String.Empty Then
        '    xmldoc.Load(filePath & FileUpload1.FileName)
        '    xmlNodeList = xmldoc.SelectNodes("/products/product")

        '    Dim oProduct As New MSDProduct()

        '    For Each xmlNode In xmlNodeList
        '        oProduct.Clear()
        '        Try
        '            oProduct.LoadFromXML(xmlNode)
        '            ContentID = AddProduct(oProduct)
        '        Catch ex As System.NullReferenceException
        '            errorMessage.Text = "Node not found"
        '            'ToDo: log message
        '        End Try


        '        'Assign taxonomy
        '        'ToDo: add error handling
        '        If Not ContentID = 0 Then
        '            AddContentIDtoTaxonomy(ContentID, TAXONOMY_ID)
        '        End If
        '    Next
        'End If
    End Sub
    'Add product
    Private Function AddProduct(ByVal oProduct As MSDProduct) As Long
        Dim ContentID As Long
        Dim ProductXML As String
        Dim productComment As String = "This is part of the mass upload."
        Dim productSummaryHTML As String
        Dim metaInfoXML As String
        Dim api As New Ektron.Cms.API.Content.Content
        Dim messLogger As New MessageLogger(HttpContext.Current.Server.MapPath(LOG_PATH))

        ProductXML = BuildProductInfoXML(oProduct)
        productSummaryHTML = ProductXML
        metaInfoXML = BuildMetaDataXML(oProduct)

        Try
            errorMessage.Text = errorMessage.Text & "<BR>"
            ContentID = api.AddContent(oProduct.ContentTitle, _
                            productComment, _
                            ProductXML, _
                            String.Empty, _
                            productSummaryHTML, _
                            CONTENT_LANGUAGE, _
                            oProduct.FolderID, _
                            "", _
                            "", _
                            metaInfoXML)
            'Success
            '''''''''''''
            'ContentID = api.AddContent("My title", _
            '                "my comment", _
            '                "my html", _
            '                "", _
            '                "my summary", _
            '                "1033", _
            '                0, _
            '                "", _
            '                "", _
            '                Nothing)
            ''''''''''''

            messLogger.LogMessage("Success: " & ContentID)
            Return (ContentID)
        Catch ex As Exception
            errorMessage.Text = ex.Message
            messLogger.LogMessage(ex.Message)

            Return (0)
        End Try


    End Function
    Private Function UpdateProduct(ByVal oProduct As MSDProduct, ByVal UpdateMetadata As Boolean) As Boolean
        Dim ProductXML As String
        Dim productComment As String = "This is part of the mass update."
        Dim productSummaryHTML As String
        Dim api As New Ektron.Cms.API.Content.Content
        Dim ContentData As New Ektron.Cms.ContentData
        Dim ContentEditData As New Ektron.Cms.ContentEditData
        Dim ContentMetaData() As Ektron.Cms.ContentMetaData
        Dim UpdateResult As Boolean
        Dim messLogger As New MessageLogger(HttpContext.Current.Server.MapPath(LOG_PATH))

        ProductXML = BuildProductInfoXML(oProduct)
        productSummaryHTML = ProductXML

        Try
            errorMessage.Text = errorMessage.Text & "<BR>"

            ContentData = api.GetContent(oProduct.ContentID, Ektron.Cms.Content.EkContent.ContentResultType.Staged)
            api.CheckOutContent(oProduct.ContentID)
            'Get current data
            ContentEditData = api.GetContentForEditing(oProduct.ContentID)
            ContentEditData.Title = oProduct.PartNumber & " - " & oProduct.Name
            ContentMetaData = ContentEditData.MetaData

            'Replace it with the new data
            ContentEditData.Html = ProductXML
            If UpdateMetadata = True Then
                api.UpdateContentMetaData(oProduct.ContentID, META_REPLACEMENTPARTS, oProduct.ReplacementParts)
                api.UpdateContentMetaData(oProduct.ContentID, META_REQUIREDCOMPONENTS, oProduct.RequiredComponents)
                api.UpdateContentMetaData(oProduct.ContentID, META_ACCESSORIES, oProduct.Accessories)
            End If

            ''Go through each metadata item and update only the relevant ones
            'If (ContentMetaData.Length > 0) Then
            '    For Each ContentMetaDataItem In ContentMetaData
            '        Select Case ContentMetaDataItem.TypeId
            '            Case META_REPLACEMENTPARTS
            '                api.UpdateContentMetaData(oProduct.ContentID, META_REPLACEMENTPARTS, oProduct.ReplacementParts)
            '            Case META_REQUIREDCOMPONENTS
            '                api.UpdateContentMetaData(oProduct.ContentID, META_REQUIREDCOMPONENTS, oProduct.RequiredComponents)
            '            Case META_ACCESSORIES
            '                api.UpdateContentMetaData(oProduct.ContentID, META_ACCESSORIES, oProduct.Accessories)
            '        End Select
            '    Next

            'End If

            'Save and publish it
            api.SaveContent(ContentEditData)
            api.PublishContent(oProduct.ContentID, ContentEditData.FolderId, ContentEditData.LanguageId, "", ContentEditData.UserId, "")

            'ToDo: Log success

            UpdateResult = True

        Catch ex As Exception
            errorMessage.Text = ex.Message
            messLogger.LogMessage("UpdateProduct - " & "PartNumber:" & oProduct.PartNumber & " - " & ex.Message)
            UpdateResult = False
        End Try

        Return (UpdateResult)
    End Function

    Private Function ConvertToStoreProduct(ByVal oProduct As MSDProduct) As Long
        Dim Currency_USD As Integer = 840
        Dim catalogApi As New CatalogEntryApi()
        Dim productTypeApi As New ProductTypeApi()
        Dim myInventory As New InventoryApi
        Dim myinventorydata As New InventoryData
        Dim currentUserId As Long = catalogApi.UserId
        If currentUserId > 0 Then
            Dim folderId As Long = 355
            Dim productTypeId As Long = 21
            Dim taxClassId As Long = 1
            Dim price As Decimal = 50
            Dim entryData As EntryData = Nothing
            errorMessage.Text = "Catalog Entries </br>"

            entryData = New ProductData(catalogApi.ContentLanguage)
            entryData.Title = oProduct.ContentTitle
            entryData.Sku = oProduct.PartNumber
            entryData.TemplateId = 81
            entryData.TaxClassId = taxClassId
            entryData.ProductType = New ProductTypeData()
            entryData.ProductType.Id = productTypeId
            entryData.Pricing = New PricingData(Currency_USD, price, price)
            entryData.Html = Server.HtmlDecode("<root><Picture><img alt=""Chair"" src=""/uploadedImages/chair.jpg?n=6271"" /></Picture><Title>" & entryData.Title & "</Title><Description><p>" & entryData.Title & "</p></Description></root>")
            entryData.FolderId = folderId
            catalogApi.Add(entryData)
            myinventorydata.EntryId = entryData.Id
            myinventorydata.UnitsInStock = 100
            myinventorydata.UnitsOnOrder = 10
            myinventorydata.ReorderLevel = 50
            myInventory.SaveInventory(myinventorydata)
            errorMessage.Text += " ID: " & entryData.Id & "</br>"

        Else
            errorMessage.Text = "Not logged in."
        End If
    End Function

    Protected Sub btnEcomm_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEcomm.Click
        Dim oProduct As New MSDProduct



        'Dim ContentID As Long
        'Dim ProductXML As String
        'Dim productComment As String = "This is part of the mass upload."
        'Dim productSummaryHTML As String
        'Dim metaInfoXML As String
        'Dim api As New Ektron.Cms.API.Content.Content


        'Dim messLogger As New MessageLogger(HttpContext.Current.Server.MapPath(LOG_PATH))

        'ProductXML = BuildProductInfoXML(oProduct)
        'productSummaryHTML = ProductXML
        'metaInfoXML = BuildMetaDataXML(oProduct)

        'Try
        '    errorMessage.Text = errorMessage.Text & "<BR>"
        '    ContentID = api.AddContent(oProduct.ContentTitle, _
        '                    productComment, _
        '                    ProductXML, _
        '                    String.Empty, _
        '                    productSummaryHTML, _
        '                    CONTENT_LANGUAGE, _
        '                    oProduct.FolderID, _
        '                    "", _
        '                    "", _
        '                    metaInfoXML)

        '    messLogger.LogMessage("Success: " & ContentID)
        '    Return (ContentID)
        'Catch ex As Exception
        '    errorMessage.Text = ex.Message
        '    messLogger.LogMessage(ex.Message)

        '    Return (0)
        'End Try
    End Sub

    
End Class
