DROP TABLE IF EXISTS `claim_types`;

CREATE TABLE `claim_types`  (
  `id` char(36) NOT NULL COMMENT '主键',
  `name` varchar(50) NULL COMMENT '名称',
  `display_name` varchar(50) NULL COMMENT '显示名称',
  `type` varchar(20) NULL COMMENT '类型。String, Int, DateTime, Boolean, Enum',
  `description` varchar(500) NULL COMMENT '描述',
  `required` bit NULL COMMENT '是否必须项',
  `non_editable` bit NULL COMMENT '用户是否可编辑',
  `created` datetime NULL COMMENT '创建时间',
  `updated` datetime NULL COMMENT '更新时间',
  PRIMARY KEY (`id`)
);

