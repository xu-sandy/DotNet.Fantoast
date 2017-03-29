using FCake.Domain.Entities;
using FCake.Domain.Mapping;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Configuration;
namespace FCake.Domain
{
    public partial class EFDbContext : DbContext
    {
        public EFDbContext()
            : base(ConfigurationManager.AppSettings["ConnectionString"].ToString())
        {
            ///Leo: disable the Lazy Loading the WCF will not be able to serilize the entities.
            //是否启用延迟加载:  
            //  true:   延迟加载（Lazy Loading）：获取实体时不会加载其导航属性，一旦用到导航属性就会自动加载  
            //  false:  直接加载（Eager loading）：通过 Include 之类的方法显示加载导航属性，获取实体时会即时加载通过 Include 指定的导航属性  
            this.Configuration.LazyLoadingEnabled = false;

            this.Configuration.ProxyCreationEnabled = false;

            //UseLegacyPreserveChangesBehavior
            //确定是否使用旧的行为， true 使用，false 不使用；
            this.Configuration.AutoDetectChangesEnabled = true;  //自动监测变化，默认值为 true 
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ///移除EF映射默认给表名添加“s“或者“es”
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            ///配置关系
            modelBuilder.Configurations
                //系统权限
                .Add(new UsersMap())
                .Add(new RolesMap())
                .Add(new MenusMap())
                .Add(new UserRolesMap())
                .Add(new PermissionsMap())
                .Add(new RolePermissionsMap())
                .Add(new BaseFilesMap())
                .Add(new ReviewStatusLogMap())
                .Add(new OperationLogMap())

                //业务基础配置信息
                .Add(new SysConfigMap())
                .Add(new SysTempMap())

                //产品
                .Add(new OrdersMap())
                .Add(new OrderDetailsMap())
                .Add(new ProductsMap())
                .Add(new SubProductsMap())
                .Add(new GiftCardDetailMap())
                .Add(new HotProductMap())
                //客户
                .Add(new CustomersMap())
                .Add(new GiftCardsMap())
                //订单批次
                .Add(new OrderBatchsMap())
                .Add(new OrderExceptionMap())
                //厨房制做
                .Add(new KitchenBatchsMap())
                //厨房制作从
                .Add(new KitchenBatchDetailsMap())
                //物流配送
                .Add(new DistributionMap())
                //发票
                .Add(new InvoiceMap())
                //幻灯片
                .Add(new SlideMap())
                //购物车
                .Add(new CartMap())
                //公司合作
                .Add(new CooperationMap())
                //退款管理
                .Add(new OrderRefundRecordsMap())
                //微信预支付
                .Add(new PrePayInfoMap())
                //优惠券
                .Add(new CouponsMap())
                //优惠券明细
                .Add(new CouponDetailMap())
                //手机验证码
                .Add(new PhoneCodeMap())
                //会员等级
                .Add(new MemberLevelMap())
                //会员积分记录
                .Add(new MemberIntegralLogMap())
                //会员成长
                .Add(new MemberGrowthValueLogMap())
                //产品活动
                .Add(new ProductActivityMap())
                //产品活动详情
                .Add(new ProductActivityDetailMap())
                //产品活动池
                .Add(new ProductActivityDetailPoolMap())
                //系统日志
                .Add(new SysLogMap());

            base.OnModelCreating(modelBuilder);
        }

        //表空间
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }//用户角色
        public DbSet<Menu> Menus { get; set; } //菜单
        public DbSet<User> Users { get; set; } //用户
        public DbSet<Role> Roles { get; set; } //角色   
        public DbSet<ReviewStatusLog> ReviewStatusLog { get { return Set<ReviewStatusLog>(); } } //审批数据记录表    
        public DbSet<OperationLog> OperationLogs { get; set; } //操作日志表    
        public DbSet<Customers> Customers { get; set; }//客户  
        public DbSet<Product> Products { get; set; }//产品
        public DbSet<SubProduct> SubProducts { get; set; }//子产品
        public DbSet<Orders> Orders { get; set; }//订单
        public DbSet<CustomerAddress> CustomerAddress { get; set; }//客户具体地址
        public DbSet<BaseFile> BaseFiles { get; set; }  //文件表
        public DbSet<OrderDetails> OrderDetails { get; set; }//订单详情表
        public DbSet<OrderDetailHist> OrderDetailHists { get; set; }//订单详情历史表
        public DbSet<OrderHist> OrderHists { get; set; }//订单历史
        public DbSet<VCustomOrders> VCustomOrders { get; set; } //客户定位查询视图    
        public DbSet<GiftCardDetail> GiftCardDetail { get; set; }//代金卡明细
        public DbSet<GiftCards> GiftCards { get; set; }//代金卡表
        public DbSet<VCoupons> VCoupons { get; set; }//代金卡信息视图
        public DbSet<DictionaryType> DictionaryType { get; set; }//新的字典表
        public DbSet<DictionaryData> DictionaryData { get; set; }//新的字典值表
        public DbSet<LogisticsSite> LogisticsSite { get; set; }//站点自提表
        public DbSet<Invoice> Invoices { get; set; }//发票
        public DbSet<Distribution> Distribution { get; set; }//物流配送表
        public DbSet<OrderBatch> OrderBatchs { get; set; }//订单批次表
        public DbSet<KitchenMake> KitchenMakes { get; set; }//厨房制作
        public DbSet<KitchenMakeDetail> KitchenMakeDetails { get; set; }//厨房制作从表
        public DbSet<OrderException> OrderExceptions { get; set; } //订单异常描述表
        public DbSet<Slide> Slides { get; set; } //幻灯片
        public DbSet<Cooperation> Cooperations { get; set; } //公司合作
        public DbSet<Cart> Carts { get; set; } //购物车
        public DbSet<HotProduct> HotProducts { get; set; } //热卖产品
        public DbSet<PhoneCode> PhoneCodes { get; set; } //手机验证码
        public DbSet<MsgTemplate> MsgTemplates { get; set; }//短信模版
        public DbSet<PayLog> PagLog { get; set; }//记录支付回调
        /// <summary>
        /// 业务基础配置信息
        /// </summary>
        public DbSet<SysConfig> SysConfigs { get; set; }
        /// <summary>
        /// 系统临时数据信息表
        /// </summary>
        public DbSet<SysTemp> SysTemps { get; set; }
        /// <summary>
        /// 退款管理
        /// </summary>
        public DbSet<OrderRefundRecord> OrderRefundRecords { get; set; }
        /// <summary>
        /// 优惠券
        /// </summary>
        public DbSet<Coupons> Coupons { get; set; }
        /// <summary>
        /// 优惠券明细
        /// </summary>
        public DbSet<CouponDetail> CouponDetails { get; set; }
        /// <summary>
        /// 微信预支付表
        /// </summary>
        public DbSet<PrePayInfo> PrePayInfo { get; set; }
        /// <summary>
        /// 会员等级
        /// </summary>
        public DbSet<MemberLevel> MemberLevel { get; set; }
        /// <summary>
        /// 会员积分记录
        /// </summary>
        public DbSet<MemberIntegralLog> MemberIntegralLog { get; set; }
        /// <summary>
        /// 会员成长值记录
        /// </summary>
        public DbSet<MemberGrowthValueLog> MemberGrowthValueLog { get; set; }
        /// <summary>
        /// 产品活动
        /// </summary>
        public DbSet<ProductActivity> ProductActivity { get; set; }
        /// <summary>
        /// 产品活动详情
        /// </summary>
        public DbSet<ProductActivityDetail> ProductActivityDetail { get; set; }
        /// <summary>
        /// 产品活动详情池
        /// </summary>
        public DbSet<ProductActivityDetailPool> ProductActivityDetailPool { get; set; }
        /// <summary>
        /// 系统日志
        /// </summary>
        public DbSet<SysLog> SysLog { get; set; } 
    }
}