namespace MiniSoftware.Extensions
{
    using DocumentFormat.OpenXml.Wordprocessing;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Dynamic;
    using System.Linq;
    using System.Text;

    internal static class OpenXmlExtension
    {
        /// <summary>
        /// �߼��������õ���������������ַ���
        /// </summary>
        /// <param name="paragraph">����</param>
        /// <returns>Item1�������ı���Item2���飻Item3�����ı�</returns>
        internal static List<Tuple<string, List<Run>, List<Text>>> GetContinuousString(this Paragraph paragraph)
        {
            List<Tuple<string, List<Run>, List<Text>>> tuples = new List<Tuple<string, List<Run>, List<Text>>>();
            if (paragraph == null)
                return tuples;

            var sb = new StringBuilder();
            var runs = new List<Run>();
            var texts = new List<Text>();

            //���䣺�����Ӽ�
            foreach (var pChildElement in paragraph.ChildElements)
            {
                //��
                if (pChildElement is Run run)
                {
                    //�ı���
                    if (run.IsText())
                    {
                        var text = run.GetFirstChild<Text>();
                        runs.Add(run);
                        texts.Add(text);
                        sb.Append(text.InnerText);
                    }
                    else
                    {
                        if (runs.Any())
                            tuples.Add(new Tuple<string, List<Run>, List<Text>>(sb.ToString(), runs, texts));

                        sb = new StringBuilder();
                        runs = new List<Run>();
                        texts = new List<Text>();
                    }
                }
                //��ʽ����ǩ...
                else
                {
                    //����������
                    if (pChildElement is BookmarkStart || pChildElement is BookmarkEnd)
                    {

                    }
                    else
                    {
                        if (runs.Any())
                            tuples.Add(new Tuple<string, List<Run>, List<Text>>(sb.ToString(), runs, texts));

                        sb = new StringBuilder();
                        runs = new List<Run>();
                        texts = new List<Text>();
                    }
                }
            }

            if (runs.Any())
                tuples.Add(new Tuple<string, List<Run>, List<Text>>(sb.ToString(), runs, texts));

            sb = new StringBuilder();
            runs = new List<Run>();
            texts = new List<Text>();

            return tuples;
        }

        /// <summary>
        /// �����ַ����������ַ�������
        /// </summary>
        /// <param name="texts">�����ַ�����</param>
        /// <param name="text">�������ַ���</param>
        internal static void TrimStringToInContinuousString(this IEnumerable<Text> texts, string text)
        {
            /*
            //�����Ϊ��[A][BC][DE][FG][H]
            //�����滻��[AB][E][GH]
            //�Ż���Ϊ��[AB][C][DE][FGH][]
             */

            var index = string.Concat(texts.SelectMany(o => o.Text)).IndexOf(text);
            if (index > 0)
            {
                int i = -1;
                int addLengg = 0;
                bool isbr = false;
                foreach (var textWord in texts)
                {
                    if (addLengg > 0)
                    {
                        isbr = true;
                        var leng = textWord.Text.Length;

                        if (addLengg - leng > 0)
                        {
                            addLengg -= leng;
                            textWord.Text = "";
                        }
                        else if (addLengg - leng == 0)
                        {
                            textWord.Text = "";
                            break;
                        }
                        else
                        {
                            textWord.Text = textWord.Text.Substring(addLengg);
                        }
                    }
                    else if (isbr)
                    {
                        break;
                    }
                    else
                    {
                        i += textWord.Text.Length;
                        //��ʼ����
                        if (i >= index)
                        {
                            //ȫ������
                            if (textWord.Text.Contains(text))
                            {
                                break;
                            }
                            //���ְ���
                            else
                            {
                                var str1 = textWord.Text.Substring(0, i - index + 1);
                                if (i == index)
                                    str1 = "";

                                var str2 = str1 + text;

                                addLengg = str2.Length - textWord.Text.Length;
                                textWord.Text = str2;
                            }
                        }
                    }
                }
            }
        }


        internal static bool IsText(this Run run)
        {
            return run.Elements().All(o => o is Text || o is RunProperties);
        }
    }
}