﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Net;
using System.IO;
using System.Collections;
using Com.Aote.Logs;
using Card;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Card
{
    //服务类
    public class GenericService : ICardService
    {
        private static Log Log = Log.GetInstance(typeof(GenericService));

        //加载的厂家动态库
        private CardInfos Cards;

        //端口号
        private short Port;

        //波特率
        private int Baud;

        #region 错误状态表
        //错误状态表
        private string[] Errors = 
        {
            "不是注册用户,请注册", //-1
            "端口初始化失败", //-2
            "读设备状态失败", //-3
            "无卡",          //-4
            "读卡密码次数失败", //-5
            "该卡已经损坏",     //-6
            "读卡错误",         //-7
            "该卡不是用户卡",  //-8
            "核对密码错误",   //-9
            "写卡失败",         //-10
            "备份气量不正确",  //-11
            "关闭通讯端口失败", //-12
            "该卡可能是新卡",   //-13
            "该卡非本系统卡",      //-14
            "该卡不是新卡",       //-15
            "用户卡号（地区代码）与卡内的值不匹配",   //-16
            "清卡失败",         //-17
            "气量超限",          //-18
            "卡插反"            //-19
        };
        #endregion


        public GenericService(CardInfos ci, short port, int br)
        {
            this.Cards = ci;
            this.Port = port;
            this.Baud = br;
        }

        private ICard GetCard(string name)
        {
            //未加载，先加载
            ICard card = Cards.GetCardInfo(name).Card;
            if (card == null)
            {
                throw new Exception("No such card error: " + name);
            }
            return card;
        }


        public string Test(string name)
        {
            return "none";
        }

        #region 卡操作
        //写新卡
        public WriteRet WriteNewCard(
            string factory,     //厂家代码
            string kmm,     //卡密码，写卡后返回新密码
            Int16 kzt,          //卡状态，0开户卡，1用户卡
            string kh,          //卡号
            string dqdm,        //地区代码，从气表管理里取
            string yhh,         //用户号，档案中自己输入
            string tm,          //条码，传用户档案里的条码
            Int32 ql,           //气量
            Int32 csql,         //上次购气量，有些表需要传
            Int32 ccsql,        //上上次购气量，有些表需要传
            Int16 cs,           //购气次数
            Int32 ljgql,        //当前表累计购气量
            Int16 bkcs,         //补卡次数，用户档案里保存补卡次数
            Int32 ljyql,        //累计用气量，有些表要累加原来用气量
            Int32 bjql,         //报警气量
            Int32 czsx,         //充值上限，可以在气表管理中设置
            Int32 tzed,         //透支额度，可以在气表管理中设置
            string sqrq,        //售气日期，格式为YYYYMMDD
            string cssqrq,      //上次售气日期，格式为YYYYMMDD
            Int32 oldprice,     //旧单价，价格管理中取
            Int32 newprice,     //新单价，价格管理中取
            string sxrq,        //生效日期，价格管理中取
            string sxbj         //生效标记，0不生效，1生效，价格管理中取
            )
        {
            Log.Debug("发卡或者补卡 start" + "cardid-" + kh + "kmm-" + kmm + "sqrq-" + sqrq + "czsx-" + czsx + "bjql-" + bjql + "gmje-" + ql + "buyprice-" + oldprice + "kzt-" + kzt);
            WriteRet ret = new WriteRet();
            try
            {
                ICard card = GetCard(factory);
                Log.Debug("factory :" + factory);
                int r = card.WriteNewCard(Port, Baud, ref kmm, kzt, kh, dqdm, yhh, tm, ql, csql, ccsql, cs, ljgql, bkcs,
                    ljyql, bjql, czsx, tzed, sqrq, cssqrq, oldprice, newprice, sxrq, sxbj);
                if (r < 0)
                {
                    if (r <= -19)
                    {
                        ret.Err = "超出错误范围，请查看表厂资料！";
                    }
                    else
                    {
                        ret.Err = Errors[-r - 1];
                    }
                }
                else
                {

                    ret.Kmm = kmm;
                    Log.Debug("kmm :" + ret.Kmm);
                }
                return ret;
            }
            catch (Exception e)
            {
                Log.Debug("发卡 exception: " + e.Message);
                ret.Exception = e.Message;
                return ret;
            }
        }

        //写购气卡
        public WriteRet WriteGasCard(
            string factory,     //厂家
            string kmm,     //卡密码，写卡后返回新密码
            string kh,          //卡号
            string dqdm,        //地区代码，从气表管理里取
            Int32 ql,           //气量
            Int32 csql,         //上次购气量，有些表需要传
            Int32 ccsql,        //上上次购气量，有些表需要传
            Int16 cs,           //购气次数
            Int32 ljgql,        //当前表累计购气量
            Int32 bjql,         //报警气量
            Int32 czsx,         //充值上限，可以在气表管理中设置
            Int32 tzed,         //透支额度，可以在气表管理中设置
            string sqrq,        //售气日期，格式为YYYYMMDD
            string cssqrq,      //上次售气日期，格式为YYYYMMDD
            Int32 oldprice,     //旧单价，价格管理中取
            Int32 newprice,     //新单价，价格管理中取
            string sxrq,        //生效日期，价格管理中取
            string sxbj         //生效标记，0不生效，1生效，价格管理中取
            )
        {
            Log.Debug("SellGas start");
            WriteRet ret = new WriteRet();
            try
            {
                ICard card = GetCard(factory);
                int r = card.WriteGasCard(Port, Baud, ref kmm, kh, dqdm, ql, csql, ccsql, cs, ljgql, bjql, czsx,
                    tzed, sqrq, cssqrq, oldprice, newprice, sxrq, sxbj);
                if (r < 0)
                {
                    if (r <= -19)
                    {
                        ret.Err = "超出错误范围，请查看表厂资料！";
                    }
                    else
                    {
                        ret.Err = Errors[-r - 1];
                    }
                }
                else
                {
                    ret.Kmm = kmm;
                }
                return ret;
            }
            catch (Exception e)
            {
                Log.Debug("SellGas exception: " + e.Message);
                ret.Exception = e.Message;
                return ret;
            }
        }

        //读卡
        public CardInfo ReadCard()
        {
            Log.Debug("ReadCard start");
            CardInfo ret = new CardInfo();
            try
            {
                //检查卡的初始状态
                int result = MingHua.CheckCard(Port, Baud);
                //有错误，显示错误内容，不是新卡不当做错误
                if (result != 0 && result != -15)
                {
                    //获取错误代码
                    if (result <= -20)
                    {
                        ret.Err = "超出错误范围，请查看表厂资料！";
                    }
                    else
                    {
                        ret.Err = Errors[-result - 1];
                    }
                    return ret;
                }

                if (Cards == null)
                {
                    throw new Exception("Cards is null");
                }
                //循环调用所有厂家的
                foreach (CardConfig info in Cards)
                {
                    ICard card = info.Card;
                    //如果不是本厂家的，看下一个
                    int r = card.CheckGasCard(Port, Baud);
                    Log.Debug("check " + info.Name + " is " + r);
                    if (r != 0)
                    {
                        continue;
                    }
                    //读卡
                    string kh = "";
                    string yhh = "";
                    Int32 ql = 0;
                    decimal money = 0;
                    Int16 cs = 0;
                    Int16 bkcs = 0;
                    r = card.ReadGasCard(Port, Baud, ref kh, ref ql, ref money, ref cs, ref bkcs, ref yhh);
                    Log.Debug("用户号：" + yhh);
                    if (r < 0)
                    {
                        if (r <= -19)
                        {
                            ret.Err = "超出错误范围，请查看表厂资料！";
                        }
                        else
                        {
                            ret.Err = Errors[-r - 1];
                        }
                    }
                    else
                    {
                        //返回读取结果
                        ret.Factory = info.Name;
                        ret.CardID = kh;
                        ret.Gas = ql;
                        ret.Money = money;
                        ret.Times = cs;
                        ret.RenewTimes = bkcs;
                        ret.Yhh = yhh;
                    }
                    return ret;
                }
                //一个都没有找到
                ret.Err = "未知厂家";
                return ret;
            }
            catch (Exception e)
            {
                Log.Debug("ReadCard exception");
                ret.Exception = e.Message;
                Log.Debug("异常：" + e.Message);
                return ret;
            }
        }

        //格式化卡
        public Ret FormatGasCard(
            string factory,     //厂家
            string kmm,         //卡密码，写卡后返回新密码
            string kh,          //卡号
            string dqdm         //地区代码，从气表管理里取
            )
        {
            Ret ret = new Ret();
            try
            {
                ICard card = GetCard(factory);
                int r = card.FormatGasCard(Port, Baud, kmm, kh, dqdm);
                if (r < 0)
                {
                    if (r <= -19)
                    {
                        ret.Err = "超出错误范围，请查看表厂资料！";
                    }
                    else
                    {
                        ret.Err = Errors[-r - 1];
                    }
                }
                return ret;
            }
            catch (Exception e)
            {
                Log.Debug("FormatCard exception");
                ret.Exception = e.Message;
                return ret;
            }
        }

        //航天解锁
        public Ret OpenCard(
         string factory,     //厂家
         string kmm,         //卡密码，写卡后返回新密码
         string kh,          //卡号
         string dqdm         //地区代码，从气表管理里取
         )
        {
            Ret ret = new Ret();
            try
            {
                ICard card = GetCard(factory);
                int r = card.OpenCard(Port, Baud);
                if (r < 0)
                {
                    if (r <= -20)
                    {
                        ret.Err = "超出错误范围，请查看表厂资料！";
                    }
                    else
                    {
                        ret.Err = Errors[-r - 1];
                    }
                }
                return ret;
            }
            catch (Exception e)
            {
                Log.Debug("OpenCard exception");
                ret.Exception = e.Message;
                return ret;
            }
        }
        #endregion
    }
}
