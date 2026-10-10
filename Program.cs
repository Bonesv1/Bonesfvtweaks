using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Collections.Generic;

namespace BonesTweaks;
record Account(string Username, string Salt, string Hash);\nrecord BackupValue(bool Exists, string? Value, RegistryValueKind Kind);
record Setting(string Category, string Name, string Description, string Path, string Value, object Data, bool Pro=false);
internal static class Program {
 [STAThread] static void Main() { ApplicationConfiguration.Initialize(); Application.Run(new MainWindow()); }
}
public sealed class MainWindow : Form {
 const string PublicKey=@"<RSAKeyValue><Modulus>t/3sNAF1o8eTT+6aVJum8jILQRVk7f9lG9269P/nQCRzNgsNQ4h3ptyN+VlQFBTWXKHVc6cOnWduyRtzxEQG+xq+Dn0zf2cgBu1mpBROq0bkxoDpUfPbFXcIAl4uln8wSniz/S2PMMgBQ0NFRCL5Pon4Nv2rgFqwofz/ufZJ04S0l4/71Y5k0iGgiiW5zvpDZQ35GCfgyyYxeI+9ZV/BEY36ZLofT1/xwnt7medj66gUmzGUo+sxgzdRTleUSt8cXM1fGb4Pb2R87wcqRze+LPCILQqmXhfrA5Y4x8YyPApW6+4oTy6h+WgpjsgxHpkH/TSqEwDNy2yKG1pn3ecrChpDMiyDeQat4LJJUTDEY41sE9UbvO9nn2DBgRl9TnIY3QxlkJbjhOEcDkQb6ScLAoraAhFkjrCFTg0bUjQKAxUwfpp9+9CvlUZryi8l8XGjfLuJeLTkWa86JYPVuxnXF18w5epQ8oznbfVJYS8qIG8c9rWxMZCSVtcweUm2TI7D</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
 readonly string dir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"BonesTweaks");
 readonly Color bg=Color.FromArgb(17,11,27), panel=Color.FromArgb(35,23,52), purple=Color.FromArgb(126,73,186);
 readonly FlowLayoutPanel list=new(){Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(14)};
 readonly Label title=new(){Text="OVERVIEW",ForeColor=Color.White,Font=new Font("Segoe UI",20,FontStyle.Bold),AutoSize=true};
 readonly Label status=new(){Text="Sign in to access tweaks",ForeColor=Color.FromArgb(198,170,220),AutoSize=true};
 string user=""; bool pro=false;
 readonly Setting[] settings={
 new("FPS & Gaming","Windows Game Mode","Enables Game Mode; FPS gains are not guaranteed.",@"Software\\Microsoft\\GameBar","AutoGameModeEnabled",1),
 new("FPS & Gaming","Disable background capture","Stops Game DVR background recording; recording features may be affected.",@"System\\GameConfigStore","GameDVR_Enabled",0),
 new("FPS & Gaming","Disable app capture","Disables Windows game app capture.",@"Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR","AppCaptureEnabled",0),
 new("FPS & Gaming","Hide Game Bar startup tips","Hides Xbox Game Bar startup panel.",@"Software\\Microsoft\\GameBar","ShowStartupPanel",0),
 new("Input Latency","Disable mouse acceleration","Changes Windows desktop pointer acceleration, not Fortnite raw input.",@"Control Panel\\Mouse","MouseSpeed","0"),
 new("Input Latency","Mouse acceleration threshold 1","Disables the first Windows desktop pointer acceleration threshold.",@"Control Panel\\Mouse","MouseThreshold1","0"),
 new("Input Latency","Mouse acceleration threshold 2","Disables the second Windows desktop pointer acceleration threshold.",@"Control Panel\\Mouse","MouseThreshold2","0"),
 new("Input Latency","Disable mouse pointer trails","Disables desktop pointer trails; not a guaranteed gaming latency improvement.",@"Control Panel\\Mouse","MouseTrails","0"),
 new("Controller","Disable Xbox Guide Game Bar shortcut","Prevents controller Guide button from opening Game Bar on supported devices.",@"Software\\Microsoft\\GameBar","UseNexusForGameBarEnabled",0),
 new("Controller","Controller setup guidance","Use a wired connection when supported, update official firmware and adjust in-game deadzones.","","",0),
 new("Windows","Disable taskbar animations","Reduces taskbar animation effects.",@"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced","TaskbarAnimations",0),
 new("Windows","Disable window animations","Disables some minimize and maximize animations; sign out may be needed.",@"Control Panel\\Desktop\\WindowMetrics","MinAnimate","0"),
 new("Windows","Shorter menu delay","Sets desktop menu delay to 100 ms.",@"Control Panel\\Desktop","MenuShowDelay","100"),
 new("Windows","Disable transparency","Disables Windows transparency effects.",@"Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize","EnableTransparency",0),
 new("Network","Network setup guidance","Ethernet, router QoS and avoiding background downloads may help consistency. No risky TCP edits.","","",0),
 new("Pro","Desktop visual effects","Choose performance-oriented Windows desktop visual effects.",@"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects","VisualFXSetting",2,true),
 new("Pro","Quiet notifications","Disable toast notifications; important alerts may be hidden.",@"Software\\Microsoft\\Windows\\CurrentVersion\\PushNotifications","ToastEnabled",0,true)
 };
 public MainWindow(){
  Text="Bones Tweaks"; Width=1030;Height=710;MinimumSize=new Size(830,590);StartPosition=FormStartPosition.CenterScreen;
  BackColor=bg;ForeColor=Color.White;Font=new Font("Segoe UI",10);
  try{Icon=new Icon(Path.Combine(AppContext.BaseDirectory,"BonesTweaks.ico"));}catch{}
  var sidebar=new FlowLayoutPanel{Dock=DockStyle.Left,Width=220,FlowDirection=FlowDirection.TopDown,WrapContents=false,BackColor=panel,Padding=new Padding(14)};
  var brand=new PictureBox{Size=new Size(170,115),SizeMode=PictureBoxSizeMode.Zoom};
  string imagePath=Path.Combine(AppContext.BaseDirectory,"Brand.png");
  if(File.Exists(imagePath))brand.Image=Image.FromFile(imagePath);
  sidebar.Controls.Add(brand);
  sidebar.Controls.Add(new Label{Text="BONES TWEAKS",Font=new Font("Segoe UI",15,FontStyle.Bold),ForeColor=Color.White,Width=190,Height=40,TextAlign=ContentAlignment.MiddleCenter});
  foreach(var cat in new[]{"Overview","FPS & Gaming","Input Latency","Controller","Windows","Network","Pro","Restore"}){
   var b=Btn(cat,185,42);b.Margin=new Padding(2,5,0,5);b.Click+=(_,_)=>ShowCategory(cat);sidebar.Controls.Add(b);
  }
  var main=new Panel{Dock=DockStyle.Fill,Padding=new Padding(22)};
  var head=new Panel{Dock=DockStyle.Top,Height=86};
  title.Location=new Point(12,8);status.Location=new Point(14,52);head.Controls.Add(title);head.Controls.Add(status);
  main.Controls.Add(list);main.Controls.Add(head);
  Controls.Add(main);Controls.Add(sidebar);
  Shown+=(_,_)=>{if(!Login()){Close();return;}ShowCategory("Overview");};
 }
 Button Btn(string label,int w=200,int h=42)=>new(){Text=label,Width=w,Height=h,FlatStyle=FlatStyle.Flat,BackColor=purple,ForeColor=Color.White,Cursor=Cursors.Hand,Margin=new Padding(6)};
 string AccountPath=>Path.Combine(dir,"account.json");
 string CodePath=>Path.Combine(dir,"pro-code.txt");
 string BackupPath=>Path.Combine(dir,"backup-v2.json");
 bool Login(){
  Directory.CreateDirectory(dir);
  var f=new Form{Text="Bones Tweaks • Account",Width=410,Height=355,StartPosition=FormStartPosition.CenterParent,BackColor=panel,ForeColor=Color.White,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false};
  var lbl=new Label{Text=File.Exists(AccountPath)?"SIGN IN":"CREATE LOCAL ACCOUNT",Location=new Point(25,20),Width=340,Height=40,Font=new Font("Segoe UI",17,FontStyle.Bold)};
  var u=new TextBox{Location=new Point(25,88),Width=340,PlaceholderText="Username"};
  var pass=new TextBox{Location=new Point(25,143),Width=340,UseSystemPasswordChar=true,PlaceholderText="Password (10+ characters)"};
  var error=new Label{Location=new Point(25,254),Width=350,Height=40,ForeColor=Color.Salmon};
  var go=Btn(File.Exists(AccountPath)?"Login":"Create account",340,43);go.Location=new Point(25,195);
  go.Click+=(_,_)=>{
   try{
    if(File.Exists(AccountPath)){
     var a=JsonSerializer.Deserialize<Account>(File.ReadAllText(AccountPath))!;
     byte[] salt=Convert.FromBase64String(a.Salt);
     var hash=Rfc2898DeriveBytes.Pbkdf2(pass.Text,salt,200000,HashAlgorithmName.SHA256,32);
     if(u.Text!=a.Username || !CryptographicOperations.FixedTimeEquals(hash,Convert.FromBase64String(a.Hash)))throw new Exception("Incorrect login.");
    } else {
     if(!System.Text.RegularExpressions.Regex.IsMatch(u.Text,@"^[A-Za-z0-9_]{3,24}$") || pass.Text.Length<10)throw new Exception("Username 3–24 characters; password 10+.");
     byte[] salt=RandomNumberGenerator.GetBytes(24);
     var hash=Rfc2898DeriveBytes.Pbkdf2(pass.Text,salt,200000,HashAlgorithmName.SHA256,32);
     File.WriteAllText(AccountPath,JsonSerializer.Serialize(new Account(u.Text,Convert.ToBase64String(salt),Convert.ToBase64String(hash))));
    }
    user=u.Text;pro=File.Exists(CodePath)&&VerifyCode(File.ReadAllText(CodePath));
    f.DialogResult=DialogResult.OK;f.Close();
   }catch(Exception ex){error.Text=ex.Message;}
  };
  f.Controls.AddRange(new Control[]{lbl,u,pass,go,error});
  return f.ShowDialog(this)==DialogResult.OK;
 }
 static byte[] UrlDecode(string s){s=s.Replace('-','+').Replace('_','/');return Convert.FromBase64String(s.PadRight(s.Length+(4-s.Length%4)%4,'='));}
 bool VerifyCode(string code){
  try{
   var parts=code.Trim().Split('.');if(parts.Length!=3||parts[0]!="BT1")return false;
   byte[] bytes=UrlDecode(parts[1]),sig=UrlDecode(parts[2]);
   using var rsa=RSA.Create();rsa.FromXmlString(PublicKey);
   if(!rsa.VerifyData(bytes,sig,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1))return false;
   using var json=JsonDocument.Parse(bytes);
   var r=json.RootElement;
   if(r.GetProperty("product").GetString()!="bones-tweaks-pro"||r.GetProperty("user").GetString()!=user)return false;
   if(r.TryGetProperty("expires",out var expiry)&&expiry.ValueKind==JsonValueKind.String&&DateTime.TryParse(expiry.GetString(),out var dt)&&dt.ToUniversalTime()<DateTime.UtcNow)return false;
   return true;
  }catch{return false;}
 }
 void ShowCategory(string category){
  title.Text=category.ToUpperInvariant();status.Text=$"Signed in: {user}   •   Pro: {(pro?"Active":"Locked")}";
  list.Controls.Clear();
  if(category=="Overview"){AddCard("Welcome to Bones Tweaks","Choose a category. Every change has a backup and can be restored.",null);AddCard("Safety","Changes are optional. No guaranteed FPS gains. No anti-cheat, kernel, or driver changes.",null);return;}
  if(category=="Restore"){AddCard("Restore all saved changes","Returns every registry value changed by this app to its original state.",()=>RestoreAll());return;}
  if(category=="Pro"&&!pro){
   AddCard("Redeem Pro code","Creator-issued codes are bound to your local username.",()=>Redeem());
   foreach(var s in settings)if(s.Pro)AddCard("🔒 "+s.Name,"Pro required",null,"Pro required");
   return;
  }
  foreach(var s in settings)if(s.Category==category)AddCard(s.Name,s.Description,string.IsNullOrEmpty(s.Path)?()=>MessageBox.Show(s.Description,"Bones Tweaks guidance"):()=>Apply(s));
 }
 void AddCard(string heading,string description,Action? action,string? tip=null){
  var card=new Panel{Width=690,Height=116,BackColor=panel,Margin=new Padding(2,2,2,12),Padding=new Padding(14)};
  var name=new Label{Text=heading,Location=new Point(16,13),Size=new Size(470,28),Font=new Font("Segoe UI",12,FontStyle.Bold)};
  var info=new Label{Text=description,Location=new Point(16,48),Size=new Size(490,55),ForeColor=Color.FromArgb(192,171,209)};
  card.Controls.Add(name);card.Controls.Add(info);
  if(action!=null){var btn=Btn("Apply / Open",140,36);btn.Location=new Point(526,34);btn.Click+=(_,_)=>{try{action();}catch(Exception ex){MessageBox.Show(ex.Message,"Bones Tweaks");}};card.Controls.Add(btn);}
  if(tip!=null){var t=new ToolTip();t.SetToolTip(card,tip);t.SetToolTip(name,tip);}
  list.Controls.Add(card);
 }
 Dictionary<string,BackupValue> ReadBackup(){
  try{return File.Exists(BackupPath)?JsonSerializer.Deserialize<Dictionary<string,BackupValue>>(File.ReadAllText(BackupPath))??new():new();}
  catch{return new();}
 }
 void Apply(Setting s){
  if(s.Pro&&!pro)throw new Exception("Pro required.");
  var backup=ReadBackup();string id=s.Path+"|"+s.Value;
  using var key=Registry.CurrentUser.CreateSubKey(s.Path,true);
  if(!backup.ContainsKey(id)){var old=key.GetValue(s.Value);backup[id]=old==null?new BackupValue(false,null,RegistryValueKind.None):new BackupValue(true,Convert.ToString(old),key.GetValueKind(s.Value));File.WriteAllText(BackupPath,JsonSerializer.Serialize(backup));}
  key.SetValue(s.Value,s.Data,s.Data is string ? RegistryValueKind.String : RegistryValueKind.DWord);
  MessageBox.Show("Applied. Original setting backed up.","Bones Tweaks");
 }
 void RestoreAll(){
  var backup=ReadBackup();
  foreach(var item in backup){
   int split=item.Key.LastIndexOf('|');if(split<0)continue;
   using var key=Registry.CurrentUser.CreateSubKey(item.Key[..split],true);
   string value=item.Key[(split+1)..];
   if(item.Value==null)key.DeleteValue(value,false);
   else if(int.TryParse(item.Value,out int n))key.SetValue(value,n,RegistryValueKind.DWord);
  }
  File.Delete(BackupPath);MessageBox.Show("Saved registry values restored.","Bones Tweaks");
 }
 void Redeem(){
  using var f=new Form{Text="Redeem Bones Pro",Width=530,Height=190,StartPosition=FormStartPosition.CenterParent,BackColor=panel,ForeColor=Color.White};
  var t=new TextBox{Location=new Point(18,24),Width=480,PlaceholderText="Paste your creator-issued Pro code"};
  var b=Btn("Activate",160,40);b.Location=new Point(18,72);
  b.Click+=(_,_)=>{if(!VerifyCode(t.Text)){MessageBox.Show("Invalid code or wrong account.");return;}Directory.CreateDirectory(dir);File.WriteAllText(CodePath,t.Text.Trim());pro=true;f.DialogResult=DialogResult.OK;f.Close();};
  f.Controls.Add(t);f.Controls.Add(b);if(f.ShowDialog(this)==DialogResult.OK)ShowCategory("Pro");
 }
}
