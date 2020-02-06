using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace TujaEsploristo
{
    public class TujaDosierujo
    {
        private readonly string dosierujo;
        private readonly string jSonDosierujo;
        private readonly string xmlDosierujo;
        public static List<string> lingvoj = new List<string>(new[] { "fr", "ru", "cs", "hu", "de", "be", "sk", "pl", "en", "nl", "pt", "es", "ca", "it", "bg", "fa", "sv", "id", "br" });//, "fi", "tr", "he", "oc", "el", "ja", "uk", "ro", "hr", "vo", "da", "zh", "tp", "no", "la", "kek", "ia", "ie", "mk", "ku", "sr", "lat", "gd", "af", "lv", "sl", "th", "grc", "eu", "os", "is", "lt", "ar", "az", "et", "kk", "ky", "sw", "tk", "tt", "ug", "uz", "cy", "ga", "gl", "ko", "mn", "ka", "vi", "yi", "jbo", "mo", "iu", "qu", "to" });
        public static List<string> balizoj = new List<string>(new[] { "ref", "#text", "ind", "ctl", "refgrp", "klr", "frm", "sncref", "k", "g", "bib", "lok", "mll" });
        public static List<string> eksbalizoj = new List<string>(new[] { "fnt", "ekz", "trd", "uzo", "url", "#comment", "aut", "vrk" });

        public static List<string> finoj = new List<string>(new[] { "as", "is", "os", "us", "a", "e", "i", "o", "u", "j", "n" , "jn" });
        public static List<string> sufuksoj = new List<string>(new[] { "aĉ", "ad", "aĵ", "an", "ar", "ĉj", "ebl", "ec", "eg", "ej", "em", "end", "er", "estr", "et", "iĉ", "id", "ig", "iĝ", "il", "in", "ind", "ing", "ism", "ist", "nj", "obl", "on", "op", "uj", "ul", "um" });
        public static List<string> prefiksoj = new List<string>(new[] { "bo", "dis", "eks", "ek", "fi", "ge", "mal", "mis", "pra", "re" });

        public TujaDosierujo(string dosierujo)
        {
            this.dosierujo = dosierujo;
            jSonDosierujo = Path.Combine(dosierujo, "TujaEsploristoXml","JsonDosiero");
            if (!Directory.Exists(jSonDosierujo))
                Directory.CreateDirectory(jSonDosierujo);
            xmlDosierujo = Path.Combine(dosierujo, "TujaEsploristoXml", "xmlutf8");
            if (!Directory.Exists(xmlDosierujo))
                Directory.CreateDirectory(xmlDosierujo);
        }

        private void SavuJSon<T>(IEnumerable<T> records, string nom, string signature = "", bool nurjson=false)
        {
            string dosiero = Path.Combine(jSonDosierujo, nom + ".js");
            using (StreamWriter outputFile = new StreamWriter(dosiero, false, System.Text.Encoding.UTF8))
            {
                if (nurjson)
                    outputFile.WriteLine(JsonConvert.SerializeObject(records));
                else
                {
                    if (!string.IsNullOrWhiteSpace(signature))
                        outputFile.WriteLine(signature);
                    outputFile.WriteLine($"const {nom} =");
                    outputFile.WriteLine(JsonConvert.SerializeObject(records));
                    outputFile.WriteLine("module.exports = { " + nom + " }");
                }
            }
            Console.WriteLine($"SavuJSon : dosiero {dosiero} kreinta!");
        }
        private void SavuVortaronŜlosilon(List<vortaro> records, string nom)
        {
            string xmlDosierujoŝlosiloj = Path.Combine(dosierujo, "TujaEsploristoXml", "ŝlosiloj");
            if (!Directory.Exists(xmlDosierujoŝlosiloj))
                Directory.CreateDirectory(xmlDosierujoŝlosiloj);
            string dosiero = Path.Combine(xmlDosierujoŝlosiloj, nom + ".txt");
            using (StreamWriter outputFile = new StreamWriter(dosiero, false, System.Text.Encoding.UTF8))
            {
                foreach (var v in records)
                {
                    string line = string.Format("{0}-{1} : {2}", v.vorto, v.homografo, v.traduko);
                    outputFile.WriteLine(line);
                }
            }
            Console.WriteLine($"SavuVortaronŜlosilon : dosiero {dosiero} kreinta!");
        }
        private void skribuMesaĝon()
        {
            Console.WriteLine(@"!!!! Vi forgesis !!!! ");
            Console.WriteLine(@"2.3  por krei la 60.000 dosierojn, kiujn ĉi tiu apo povos uzi : ");
            Console.WriteLine(@"     > ./TujaEsploristo ujo=/Users/mi/Desktop/tuja-vortaro-master krei=utf8xml");
        }

        #region 1. ESPDIC
        public void KreiESPDIC()
        {
            //# ESPDIC (Esperanto – English Dictionary) – 20 July 2019 - Paul Denisowski (www.denisowski.org)			
            string docPath = Path.Combine(dosierujo, "espdic", "espdic.txt");
            Console.WriteLine("KreuESPDIC : " + docPath);
            var query = File.ReadAllLines(docPath)
                .Where(line => line.Contains(':'))
                .Select(line =>
                {
                    var columns = line.Split(':');
                    return new vortaro { vorto = columns[0].Trim(), traduko = columns[1].Replace("\"", "'").Trim(), homografo = "1" };
                });

            var querydoublons = query.Where(c => c.traduko.Contains(',') || c.traduko.Contains(';'));
            var querysingletons = query.Where(c => !(c.traduko.Contains(',') || c.traduko.Contains(';')));
            List<vortaro> novListe = querysingletons.ToList();
            foreach (var line in querydoublons)
            {
                var homografoj = line.traduko.Split(';');
                int homoc = 1;
                foreach (var homografo in homografoj)
                {
                    var synonymes = homografo.Split(',');
                    string prevSyno = string.Empty;
                    foreach (var synonyme in synonymes)
                    {
                        var synonym = synonyme.Replace("\"", "'").Trim();
                        if (!string.IsNullOrWhiteSpace(prevSyno))
                        {
                            if (!synonym.Contains(")"))
                                prevSyno = prevSyno + synonym + ", ";
                            else
                            {
                                novListe.Add(new vortaro { vorto = line.vorto, traduko = prevSyno + synonym, homografo = homoc.ToString() });
                                prevSyno = string.Empty;
                            }
                        }
                        else
                        if (synonym.Contains("(") && !synonym.Contains(")"))
                            prevSyno = prevSyno + synonym + ", ";
                        else
                            novListe.Add(new vortaro { vorto = line.vorto, traduko = synonym, homografo = homoc.ToString() });
                    }
                    homoc++;
                }
            }

            SavuJSon(novListe.OrderBy(v => v.vorto), "eo_espdic", @"// ESPDIC by Paul Denisowski, CC-BY-3.0.");
        }
        #endregion

        #region 2. Etimologio
        public void KreiEtimologioj()
        {
            string docPath = Path.Combine(dosierujo, "etymology", "etymology.txt");

            var query = File.ReadAllLines(docPath)
                .Where(lineo => lineo.Contains('='))
                .Select(lineo =>
                {
                    var columns = lineo.Split('=');
                    return new Etimologio { vorto = columns[0].Trim(), etimologio = columns[1].Trim() };
                });
            SavuJSon(query.OrderBy(v => v.vorto), "etimologioj", @"// Etimologio by Tuja Vortaro");
         
        }
        #endregion

        #region 3. Unikodigi
        public void Unikodiĝi()
        {
            var document = XDocument.Load(@"Data/literoj.xml");
            var unicodeList = (document.Element("literoj")?.Elements("l") ?? Enumerable.Empty<XElement>())
                .Where(c => c.Attribute("kodo").Value.Contains("#"))
                .Select(el => {
                    int code = int.Parse(el.Attribute("kodo").Value.Replace("#x", ""), System.Globalization.NumberStyles.HexNumber);
                    return new unicodage
                    {
                        nomo = el.Attribute("nomo")?.Value,
                        kodo = char.ConvertFromUtf32(code)
                    };
                }
                );
            string docPath = Path.Combine(dosierujo, "revo", "src", "xml");
            int max = 0;
            foreach (string dir in Directory.GetFiles(docPath, "*.xml"))
            {
                ConvertAllAnsiXMLToUTF8(dir, unicodeList);
                //if (max++ > 200)
                //    break;
            }
        }
        private void ConvertAllAnsiXMLToUTF8(string path, IEnumerable<unicodage> unicodeList)
        {
            var query = File.ReadAllLines(path)
                .Where(line => line.Length > 0)
                .Select(line =>
                {
                    string newLine = line;
                    while (newLine.Contains('&'))
                    {
                        int pos1 = newLine.IndexOf('&');
                        int pos2 = newLine.IndexOf(';', pos1);
                        if (pos2 < pos1)
                            return newLine;
                        string code = newLine.Substring(pos1 + 1, pos2 - pos1 - 1);
                        var lunicode = string.Empty;
                        if (code.StartsWith("#",StringComparison.Ordinal))
                        {
                            int code1 = int.Parse(code.Replace("#x", "").Replace("#", ""), System.Globalization.NumberStyles.HexNumber);
                            lunicode = char.ConvertFromUtf32(code1);
                        }
                        else
                        {
                            var trouvé = unicodeList.Where(l => l.nomo == code);
                            if (trouvé != null && trouvé.Count() > 0)
                                lunicode = trouvé.Select(l => l.kodo).Single();
                        }
                        newLine = newLine.Replace("&" + code + ";", lunicode).Replace('ȕ', '×').Replace('蜢', '-').Replace('舗', '\'').Replace('舤', '†').Replace('蜒', '∈').Replace('興', '-').Replace('蝄', '∨')
                                        .Replace('蝃', '∧').Replace('蜩', '∙').Replace('艂', '′').Replace('蠄', '≤')
                                        .Replace('ű', '«').Replace('Ƈ', '»').Replace('舖', '‘').Replace('蝇', '∫').Replace('薔', '→').Replace('蜡', '∑').Replace('蜙', '∏')
                                        .Replace('頰', '♦').Replace('蠴', '⊂').Replace('蠅', '≥').Replace('蜆', '∂').Replace('蜐', '∆').Replace('蜑', '∇').Replace('虘', '⇒')
                                        .Replace('Ų', '¬').Replace('蠵', '⊃').Replace('蜴', '∞').Replace('蜰', '√').Replace('虠', '⇔')
                                        .Replace('頱', '♧').Replace('頤', '♠').Replace('頥', '♡').Replace('頦', '♢')
                                        .Replace('蔲', '⅔').Replace('蕀', '⅜')
                                        ;
                    }
                    return newLine;
                });
            string dosiero = Path.Combine(xmlDosierujo, Path.GetFileName(path));
            using (StreamWriter outputFile = new StreamWriter(dosiero))
            {
                foreach (var l in query)
                    outputFile.WriteLine(l);
            }
            Console.WriteLine($"XMLAlUTF8 : dosiero {dosiero} kreinta!");
        }

        #endregion

        #region 4. Krei la vortarojn
        public void KreiVortarojn()
        {
            Dictionary<string, List<vortaro>> vortar =  new Dictionary<string, List<vortaro>>();
            lingvoj.ForEach(l =>
            {
                vortar.Add(l, new List<vortaro>(55000));
            });

            List<Leksemoj> leksemoj = new List<Leksemoj>(70000);

            AnalizuXMLPaĝojnKajTrovuTradukojn(vortar,leksemoj);

            lingvoj.ForEach(l =>
            {
                var dico = vortar[l].OrderBy(v => v.vorto);
                if (dico != null)
                    SavuJSon(dico, "eo_" + l);
            });

            var dicoLeks = leksemoj.OrderBy(v => v.leksemo);
            if (dicoLeks != null)
            {
                SavuJSon(dicoLeks, "leksemoj_vortos", nurjson: false);
            }
        }
        private void AnalizuXMLPaĝojnKajTrovuTradukojn(Dictionary<string, List<vortaro>> vortar, List<Leksemoj> leksemoj)
        {
            string[] dosieroj = Directory.GetFiles(xmlDosierujo, "*.xml");
            if (dosieroj == null || dosieroj.Length < 500)
            {
                skribuMesaĝon();
                return;
            }
            int max = 0;
            foreach (string path in dosieroj)
            {
                XmlDocument document = new XmlDocument();
                document.Load(path);               
                var radiko = document.SelectSingleNode("vortaro/art/kap/rad").InnerText;
                if (!string.IsNullOrWhiteSpace(radiko))
                {
                    lingvoj.ForEach(l =>
                    {
                        var dico = vortar[l];
                        if (dico != null)
                            AnalizuNodojn(document, radiko, dico, leksemoj, l, serĉuTradukojnEnNodo);
                    });
                }
                if (max++ > 100)
                    break;
            }           
        }
        public void KreiVortarojnV0()
        {
            lingvoj.ForEach(l =>
            {
                var dico = AnalizuXMLPaĝojnKajTrovuTradukojn(l);
                if (dico != null)
                    SavuJSon(dico, "eo_" + l);
            });
        }
        private IEnumerable<vortaro> AnalizuXMLPaĝojnKajTrovuTradukojn(string l)
        {
            string[] dosieroj = Directory.GetFiles(xmlDosierujo, "*.xml");
            if (dosieroj == null || dosieroj.Length < 500)
            {
                skribuMesaĝon();
                return null;
            }
            List<vortaro> dicospec = new List<vortaro>(55000);
            foreach (string path in dosieroj)
            {
                XmlDocument document = new XmlDocument();
                document.Load(path);
                var radiko = document.SelectSingleNode("vortaro/art/kap/rad").InnerText;
                //if (!string.IsNullOrWhiteSpace(radiko))
                    //AnalizuNodojn(document, radiko, dicospec, l, serĉuTradukojnEnNodo);
            }
            IEnumerable<vortaro> vortaroLingvo = dicospec;
            return vortaroLingvo.OrderBy(v => v.vorto);
        }
        private bool serĉuTradukojnEnNodo(List<vortaro> dicospec, vortoStruKt vorto, List<Leksemoj> leksemoj, string l, XmlNode node)
        {
            var trd = node.SelectNodes("trd[@lng=\"" + l + "\"]");
            foreach (XmlNode trdu in trd)
                aldonuNodoEnVortaro(dicospec, leksemoj, vorto, trdu.InnerText);
            var trdgrp = node.SelectNodes("trdgrp[@lng=\"" + l + "\"]");
            foreach (XmlNode trdgrpu in trdgrp)
            {
                var trdv = trdgrpu.SelectNodes("trd");
                foreach (XmlNode trvdgrpu in trdv)
                    aldonuNodoEnVortaro(dicospec, leksemoj, vorto, trvdgrpu.InnerText);
            }
            return true;
        }
        private void AnalizuNodojn(XmlDocument document, string radiko, List<vortaro> dicospec, List<Leksemoj> leksemoj, string l, Func<List<vortaro>, vortoStruKt, List<Leksemoj>, string, XmlNode, bool> serĉuTradukojnEnNodo)
        {
            var nodedrvList = document.SelectNodes("vortaro/art/drv");
            foreach (XmlNode nodedrv in nodedrvList)
            {
                vortoStruKt vorto = VortoStrukt(nodedrv, radiko);
                serĉuTradukojnEnNodo(dicospec, vorto, leksemoj, l, nodedrv);
                var snc = nodedrv.SelectNodes("snc");
                foreach (XmlNode sncs in snc)
                    serĉuTradukojnEnNodo(dicospec, vorto, leksemoj, l, sncs);
                var subsnc = nodedrv.SelectNodes("snc/subsnc");
                foreach (XmlNode subsncs in subsnc)
                    serĉuTradukojnEnNodo(dicospec, vorto, leksemoj, l, subsncs);
            }
        }
        private void aldonuNodoEnVortaro(List<vortaro> dicospec, List<Leksemoj> leksemoj, vortoStruKt vorto, string traduk)
        {
            traduk = traduk.Replace(System.Environment.NewLine, " ").Replace('"', '\'');
            traduk = traduk.Replace("  ", " ").Trim();
            if (traduk.Length > 0)
            {
                vortaro novavortaro = new vortaro { vorto = vorto.vorto, traduko = traduk, leksemo = vorto.radiko, homografo = "1" };
                if (!dicospec.Contains(novavortaro))
                    dicospec.Add(novavortaro);
                Leksemoj leks = new Leksemoj { leksemo = vorto.radiko, vorto = vorto.vorto };
                if (!leksemoj.Contains(leks))
                    leksemoj.Add(leks);
            }
        }
        private vortoStruKt VortoStrukt(XmlNode nodedrv, string radiko)
        {
            //    <kap><ofc>*</ofc><tld/>i</kap>
            //    <kap><tld/>iĝi, <var><kap>sin <tld/>i</kap></var></kap>
            vortoStruKt aktuaVorto = new vortoStruKt();
            var veravorto = string.Empty;
            var kap = nodedrv.SelectSingleNode("kap");
            if (kap.FirstChild.NodeType == XmlNodeType.Text)
                aktuaVorto.prefikso = kap.FirstChild.Value;
            aktuaVorto.radiko = radiko;

            bool sekvantoEstosSufikso = false;
            foreach (XmlNode infanode in kap.ChildNodes)
            {
                if (sekvantoEstosSufikso && infanode.NodeType == XmlNodeType.Text)
                {
                    aktuaVorto.sufikso = infanode.Value.Replace(",", "").Trim();
                    break;
                }
                if (infanode.Name == "tld")
                    sekvantoEstosSufikso = true;
            }

            return aktuaVorto;
        }
        #endregion

        #region 5. Krei la difinojn kaj la eksemplojn
        public void KreiDifinojn()
        {
            List<Ekzemplo> ekzemploj = new List<Ekzemplo>();
            var dicodif = AnalizuXMLPaĝonjKajTrovuDifinojnKajEkzemplojn(ekzemploj);
            if (dicodif != null)
            {
                SavuJSon(dicodif.OrderBy(v => v.vorto), "difinoj");
                SavuJSon(ekzemploj.OrderBy(v => v.vorto), "ekzemploj");
            }
        }
        #region farigi vortarojn kun definoj kaj ekzempoj
        private List<Difino> AnalizuXMLPaĝonjKajTrovuDifinojnKajEkzemplojn(List<Ekzemplo> ekzemploj)
        {
            string[] dosieroj = Directory.GetFiles(xmlDosierujo, "*.xml");
            if (dosieroj == null || dosieroj.Length < 500)
            {
                skribuMesaĝon();
                return null;
            }
            List<Difino> dicospec = new List<Difino>(55000);
            foreach (string dosiero in dosieroj)
            {
                string retafile = Path.GetFileName(dosiero);
                XmlDocument document = new XmlDocument();
                document.Load(dosiero);
                var radiko = document.SelectSingleNode("vortaro/art/kap/rad").InnerText;
                if (radiko != null)
                {
                    var nodedrvList = document.SelectNodes("vortaro/art/drv");
                    foreach (XmlNode nodedrv in nodedrvList)
                    {
                        int difno = 1;
                        vortoStruKt vorto = VortoStrukt(nodedrv, radiko);
                        serĉuDifinojnEnNodo(dicospec, ekzemploj, vorto, nodedrv, retafile, ref difno);
                        var snc = nodedrv.SelectNodes("snc");
                        foreach (XmlNode sncs in snc)
                            serĉuDifinojnEnNodo(dicospec, ekzemploj, vorto, sncs, retafile, ref difno);
                        var subsnc = nodedrv.SelectNodes("snc/subsnc");
                        foreach (XmlNode subsncs in subsnc)
                            serĉuDifinojnEnNodo(dicospec, ekzemploj, vorto, subsncs, retafile, ref difno);
                    }
                }
            }
            return dicospec;
        }
        private void serĉuDifinojnEnNodo(List<Difino> dicospec, List<Ekzemplo> ekzemploj, vortoStruKt vorto, XmlNode node, string dosiero, ref int difno)
        {
            var dif = node.SelectNodes("dif");
            foreach (XmlNode difNode in dif)
            {
                int difdif = 0;
                string diftekst = difNode.InnerText.Trim();
                if (difNode.HasChildNodes)
                {
                    StringBuilder difi = new StringBuilder();
                    bool spacoPost = true; //post tld, ne aldonu spaco
                    int antaŭaKompto = ekzemploj.Count;
                    AnalisuNodojnPorTroviDifinojKajEkzemploj(ekzemploj, vorto, difNode, difi, ref spacoPost, dosiero, difno);
                    if (ekzemploj.Count == antaŭaKompto)
                        difdif = 0;
                    else
                    {
                        difdif = difno;
                        difno++;
                    }

                    diftekst = difi.ToString();
                }

                aldonuEnDifinoj(dicospec, vorto, diftekst, dosiero, difdif, false);
            }
        }
        private void AnalisuNodojnPorTroviDifinojKajEkzemploj(List<Ekzemplo> ekzemploj, vortoStruKt vorto, XmlNode difNode, StringBuilder difi, ref bool spacoPost, string dosiero, int difno)
        {
            if (difNode.ChildNodes.Count > 1 || (difNode.ChildNodes.Count == 1 && difNode.ChildNodes[0].Name != "#text"))
            {
                foreach (XmlNode infanode in difNode)
                {
                    if (!eksbalizoj.Contains(infanode.Name))
                        AnalisuNodojnPorTroviDifinojKajEkzemploj(ekzemploj, vorto, infanode, difi, ref spacoPost, dosiero, difno);
                    else if (infanode.Name == "ekz")
                    {
                        StringBuilder defstring = new StringBuilder();
                        AnalisuNodojnPorTroviDifinojKajEkzemploj(ekzemploj, vorto, infanode, defstring, ref spacoPost, dosiero, difno);
                        aldonuEnEkzemploj(ekzemploj, vorto.vorto, defstring.ToString(), difno, dosiero);
                    }
                }
            }
            else
                serĉuDifinojnKajEkzemplojnEnNodo(vorto, difNode, difi, ref spacoPost, dosiero);

        }
        private void serĉuDifinojnKajEkzemplojnEnNodo(vortoStruKt vorto, XmlNode infanode, StringBuilder difi, ref bool spacoPost, string dosiero)
        {
            if (balizoj.Contains(infanode.Name))
                difi.Append(FFF(infanode.InnerText, ref spacoPost));
            else if (infanode.Name == "tld")
            {
                string rara = vorto.radiko;
                if (infanode.Attributes["lit"] != null)
                {
                    if (vorto.radiko.Length == 0)
                        rara = rara.ToUpper();
                    else
                        rara = char.ToUpper(rara[0]) + rara.Substring(1);
                }
                bool prefiksita = vorto.prefikso?.Length > 0;

                if (!string.IsNullOrWhiteSpace(vorto.prefikso))
                {
                    var difinoPeco = difi.ToString();
                    if (difinoPeco.Length > vorto.prefikso.Length)
                    {
                        var lastaPeco = difinoPeco.Substring(difinoPeco.Length - vorto.prefikso.Length);
                        if (!lastaPeco.Contains(vorto.prefikso))
                            prefiksita = false;
                    }
                }
                if (!prefiksita)
                    difi.AppendFormat(" {0}", rara);
                else
                    difi.Append(rara);
                spacoPost = false;
            }
            else if (infanode.Name == "lok")
                difi.AppendFormat(" ({0})", infanode.InnerText);
            else if (infanode.Name == "em")
                difi.AppendFormat(" {0} ", infanode.InnerText);
            else if (infanode.Name == "sub" || infanode.Name == "sup")
            {
                spacoPost = false;
                //Est-ce un nombre en indice?
                string indice = infanode.InnerText.Trim();
                bool estasSub = infanode.Name == "sub";
                int unicodeBase = estasSub ? 0X2080 : 0X2070;
                foreach (char c in indice)
                {
                    int unicode = 0;
                    if (int.TryParse(c.ToString(), out unicode))
                    {
                        if (!estasSub && unicode > 0 && unicode < 4)
                        {
                            if (unicode == 1)
                                unicode = 0X00B9;
                            else if (unicode == 2)
                                unicode = 0X00B2;
                            else if (unicode == 3)
                                unicode = 0X00B3;
                        }
                        else
                            unicode += unicodeBase;
                    }
                    else if (c == '+')
                        unicode = unicodeBase + 0XA;
                    else if (c == '-')
                        unicode = unicodeBase + 0XB;
                    else if (c == '=')
                        unicode = unicodeBase + 0XC;
                    else if (c == '(')
                        unicode = unicodeBase + 0XD;
                    else if (c == ')')
                        unicode = unicodeBase + 0XE;
                    //else
                    //	listeBalises.Add(string.Format("!!!!!UNICODE : Fichier {0}, Balise {1}, innerText={2}", fichier, infanode.Name, infanode.InnerText));
                    if (unicode != 0)
                        difi.Append(char.ConvertFromUtf32(unicode));
                    else
                        difi.Append(c);
                }
            }
            else
            {
                difi.Append(FFF(infanode.InnerText, ref spacoPost));
            }
        }
        private string FFF(string tekst, ref bool spacoPost)
        {
            string[] lines = tekst.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
            );
            bool unualino = true;
            StringBuilder novstr = new StringBuilder();
            foreach (string ligne in lines)
            {
                string puralino = ligne;
                if (!unualino || spacoPost)
                    puralino = puralino.Trim();
                if (puralino.Length > 0)
                {
                    if (!unualino || spacoPost)
                        novstr.AppendFormat(" {0}", puralino);
                    else
                        novstr.Append(puralino);
                    unualino = false;
                }
            }
            tekst = novstr.ToString();
            if (spacoPost)
                return string.Format(" {0}", tekst);
            spacoPost = true;
            return tekst;
        }
        private void aldonuEnEkzemploj(List<Ekzemplo> ekzemploj, string veravorto, string traduk, int difno, string fichier)
        {
            traduk = traduk.TrimEnd(':', ' ', ',', '.', ';');
            traduk = traduk.Replace(System.Environment.NewLine, " ").Replace('"', '\'').Trim();
            traduk = traduk.Replace("  ", " ").Replace(" )", ")").Replace("( ", "(");
            if (traduk.Length > 0)
            {
                string difinoKleo = string.Format("{0}{1}", veravorto, difno);
                Ekzemplo novavortaro = new Ekzemplo { vorto = veravorto, frazo = traduk, difinoKleo = difinoKleo, retavortaro = fichier };
                ekzemploj.Add(novavortaro);
            }
        }
        private void aldonuEnDifinoj(List<Difino> dicospec, vortoStruKt vorto, string traduk, string fichier, int difno, bool test = true)
        {
            traduk = traduk.TrimEnd(':', ' ', ',', '.', ';');
            traduk = traduk.Replace(System.Environment.NewLine, " ").Replace('"', '\'').Trim();
            traduk = traduk.Replace("  ", " ").Replace(" )", ")").Replace("( ", "(");
            if (traduk.Length > 0)
            {
                string ekzemploKleo = difno > 0 ? string.Format("{0}{1}", vorto.vorto, difno) : string.Empty;
                Difino novavortaro = new Difino { vorto = vorto.vorto, frazo = traduk, ekzemploKleo = ekzemploKleo, retavortaro = fichier };
                if (!test || !dicospec.Contains(novavortaro))
                    dicospec.Add(novavortaro);
            }
        }
        #endregion

        #endregion

        #region 6. Trovi vortoj kiuj mankas en la vortaroj.
        public void ŝlosiliguPorPlenumi(string lingvo = "eo_fr")
        {
            string docPath = Path.Combine(jSonDosierujo, "eo_espdic.js");
            var query = File.ReadAllLines(docPath).SkipLast(1).Skip(2).First();
            var espdic = JsonConvert.DeserializeObject<IEnumerable<vortaro>>(query);
            List<vortaro> eoListe = new List<vortaro>();
            List<vortaro> enListe = new List<vortaro>();
            int linePos = 1;
            foreach (var line in espdic)
            {
                eoListe.Add(new vortaro { vorto = linePos.ToString(), traduko = line.vorto, homografo = line.homografo });
                enListe.Add(new vortaro { vorto = linePos.ToString(), traduko = line.traduko, homografo = line.homografo });
                linePos++;
            }
            SavuVortaronŜlosilon(eoListe, "ŝlosilo_espdic_eo");
            SavuVortaronŜlosilon(enListe, "ŝlosilo_espdic_en");

            string aliaLingvoPath = Path.Combine(jSonDosierujo, lingvo + ".js");
            if (File.Exists(aliaLingvoPath))
            {
                var queryAliaLingvo = File.ReadAllLines(aliaLingvoPath).SkipLast(1).Skip(1).First();
                var aliaLingvo = JsonConvert.DeserializeObject<IEnumerable<vortaro>>(queryAliaLingvo);
                var espoLingvoListe = aliaLingvo.Select(line => line.vorto);

                List<vortaro> eoLingvoListe = new List<vortaro>();
                List<vortaro> enLingvoListe = new List<vortaro>();

                List<string> pkeyListe = new List<string>();
                foreach (var line in eoListe)
                {
                    if (!espoLingvoListe.Contains(line.traduko))
                    {
                        eoLingvoListe.Add(new vortaro { vorto = line.vorto, traduko = line.traduko, homografo = line.homografo });
                        pkeyListe.Add(line.vorto);
                    }
                }
                foreach (var line in enListe)
                {
                    if (pkeyListe.Contains(line.vorto))
                        enLingvoListe.Add(new vortaro { vorto = line.vorto, traduko = line.traduko, homografo = line.homografo });
                }
                SavuVortaronŜlosilon(eoLingvoListe, "ŝlosilo_" + lingvo + "_eo");
                SavuVortaronŜlosilon(enLingvoListe, "ŝlosilo_" + lingvo + "_" + lingvo.Substring(3));

            }
        }

        public void DeŝlosiliguPorPlenumi(string lingvo = "eo_fr")
        {
            string xmlDosierujoŝlosiloj = Path.Combine(dosierujo, "TujaEsploristoXml", "ŝlosiloj");
            if (!Directory.Exists(xmlDosierujoŝlosiloj))
                return;

            string eoPath = Path.Combine(xmlDosierujoŝlosiloj, "ŝlosilo_" + lingvo + "_eo.txt");
            string frPath = Path.Combine(xmlDosierujoŝlosiloj, "ŝlosilo_" + lingvo + "_" + lingvo.Substring(3) + ".txt");
            if (!File.Exists(eoPath) || !File.Exists(frPath))
                return;

            //line => 11-1 : abakteria
            var eoDico = File.ReadAllLines(eoPath)
                .Where(line => line.Contains(":"))
                .Select(line =>
                {
                    string[] compo = line.Split(':');
                    return new KeyValuePair<string, string>(compo[0].Trim(), compo[1].Trim());
                }).ToDictionary(x => x.Key, x => x.Value);


            //line => 11-1 : abakteria
            var frDico = File.ReadAllLines(frPath)
                .Where(line => line.Contains(":"))
                .Select(line =>
                {
                    string[] compo = line.Split(':');
                    return new KeyValuePair<string, string>(compo[0].Trim(), compo[1].Trim());
                }).ToDictionary(x => x.Key, x => x.Value);

            var compos = frDico
                .Where(x => eoDico.ContainsKey(x.Key))
                .Select(x =>
                {
                    var eoparto = eoDico[x.Key];
                    string homografo = "1";
                    if (x.Key.Contains('-'))
                        homografo = x.Key.Split('-')[1];
                    return new vortaro { vorto = eoparto, traduko = x.Value, homografo = homografo };

                });
            SavuJSon(compos.OrderBy(l=>l.vorto), lingvo + "_adicigi");
        }
        #endregion

        #region 7. Legu html
        public async Task<bool> Legu()
        {
            // http://esperanto.cri.cn/  
            // http://esperanto.cri.cn/2521/more/2549/more2549.htm 
            HttpClient client = new HttpClient();            
            var response = await client.GetAsync("http://esperanto.cri.cn/2521/more/2549/more2549.htm");
            //var response = await client.GetAsync("http://esperanto.cri.cn/html5/index_news.htm");
            var pageContents = await response.Content.ReadAsStringAsync();
            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(pageContents);
            var novajoj = pageDocument.DocumentNode.SelectNodes("//div[@class='more']//a");
            //<a href="/2521/2020/02/03/180s209184.htm" target="_blank">Usono prenis troajn kontraŭrimedojn al epidemio en Ĉinio</a>
            List<Artikoloj> artikoloj = new List<Artikoloj>();
            foreach (var novajo in novajoj)
                await LeguNovajo(client, novajo.Attributes["href"].Value, artikoloj);
            if (artikoloj.Count > 0)
                SavuJSon(artikoloj, "chine", nurjson: false);
            return true;
        }
        public void LeguTest()
        {
            string[] dosieroj = Directory.GetFiles(dosierujo, "chine.html");
            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.Load(dosieroj[0]);
            List<Artikoloj> artikoloj = new List<Artikoloj>();
           
            var titolo = pageDocument.DocumentNode.SelectSingleNode("//h1[@id='ctitle']");
            string signdate1 = pageDocument.DocumentNode.SelectSingleNode("//div[@class='signdate1']").InnerText;
            var dato = signdate1.Split('&')[0].Split(')')[1].Trim();
            DateTime date = DateTime.Parse(dato);
            string redaktoro = string.Empty;
            int red = signdate1.IndexOf("Redaktoro：");
            if (red > 0)
                redaktoro = signdate1.Substring(red+10);
            var tekstoj = pageDocument.DocumentNode.SelectNodes("//div[@id='ccontent']/p");
            List<string> p_tekst = new List<string>();
            foreach (var pp in tekstoj)
                p_tekst.Add(pp.InnerText.Replace('\n',' ').Trim());
            Artikoloj a = new Artikoloj { ŝlosilo = "chine.html", titolo = titolo.InnerText, redaktoro = redaktoro, teksto = p_tekst.ToArray(), ligilo = "chine.html", dato = date };
            artikoloj.Add(a);

            if (artikoloj.Count > 0)
                SavuJSon(artikoloj, "chine", nurjson: false);
            LeguSekvo(artikoloj);
        }

        public void LeguSekvo(List<Artikoloj> artikoloj)
        {
            List<string> leksemoj = new List<string>();
            foreach(Artikoloj a in artikoloj)
            {
                foreach (string teks in a.teksto)
                {
                    string[] titolo = teks.Split(' ', '.', ',', '-', ';');
                    foreach (string t in titolo)
                    {
                        var tt = t;                       
                        if (tt.Length < 2)
                            continue;
                        if (tt.Length > 2)
                        {
                            bool leksemigi = false;
                            finoj.ForEach((f) => { if (!leksemigi && tt.EndsWith(f)) { tt = tt.Remove(tt.Length - f.Length); leksemigi = true; } });
                        }
                        if (tt.Length > 2)
                        {
                            bool leksemigi = false;
                            sufuksoj.ForEach((f) => { if (!leksemigi && tt.EndsWith(f)) { tt = tt.Remove(tt.Length - f.Length); leksemigi = true; } });
                        }
                        if (tt.Length > 2)
                        {
                            bool leksemigi = false;
                            prefiksoj.ForEach((f) => { if (!leksemigi && tt.StartsWith(f)) { tt = tt.Substring(f.Length); leksemigi = true; } });
                        }
                        if (tt.Length >= 2 && !leksemoj.Contains(tt))
                            leksemoj.Add(tt);
                    }
                }
            }
            if (leksemoj.Count > 0)
                SavuJSon(leksemoj, "chineloks", nurjson: true);
        }

        private async Task<bool> LeguNovajo(HttpClient client, string url, List<Artikoloj> artikoloj)
        {
            var ligilo = "http://esperanto.cri.cn" + url;
            var response = await client.GetAsync(ligilo);
            var pageContents = await response.Content.ReadAsStringAsync();
            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(pageContents);



            var titolo = pageDocument.DocumentNode.SelectSingleNode("//h1[@id='ctitle']");
            string signdate1 = pageDocument.DocumentNode.SelectSingleNode("//div[@class='signdate1']").InnerText;
            var dato = pageDocument.DocumentNode.SelectSingleNode("//div[@class='signdate1']").InnerText.Split('&')[0].Split(' ')[1];
            DateTime date = DateTime.Parse(dato);
            var teksto = pageDocument.DocumentNode.SelectSingleNode("//div[@id='ccontent']");
            //Artikoloj a = new Artikoloj { titolo = titolo.InnerText, ligilo = url, dato = date, teksto = teksto.InnerText };
            //artikoloj.Add(a);

            return true;
        }


        #endregion

        #region 8. lexème leksemo
        #endregion
    }
}
