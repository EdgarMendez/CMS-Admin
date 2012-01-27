<%@Page Language="VB" Debug="true" MasterPageFile="MSDAdmin.master" AutoEventWireup="false" CodeFile="admin.aspx.vb" Inherits="admin" title="Admin Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cphMainContent" Runat="Server">
     <form id="form1" runat="server">    
        XML File:<br />
        
        <asp:FileUpload ID="FileUpload1" runat="server" />
        <br />
        
        <asp:RadioButton ID="rdNew" runat="server" GroupName="LoadType" Text="New" 
             />
        <asp:RadioButton ID="rdUpdate" runat="server" GroupName="LoadType" 
            Checked="True" Text="Update" />
        <br />
        <asp:Button ID="btnLoad" runat="server"
        Text="Load Data" />
         <br /><br /><br />
         <asp:Button ID="btnEcomm" Text="eComm" runat="server" />
         <br />
        <br />
         <asp:Button ID="btnInstructions" Text="Instructions" runat="server" />
         <br />
         <asp:Label ID="errorMessage" runat="server" Font-Bold="True" ForeColor="Maroon"></asp:Label>
         <asp:Label ID="logOutput" runat="server" />
         </form>
        
</asp:Content>

