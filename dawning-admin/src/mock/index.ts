import Mock from 'mockjs';

// 禁用所有 mock，使用真实后端 API
// import './user';
import './message-box';

// Dashboard mock - 用于工作台展示数据
import '@/views/dashboard/workplace/mock';

Mock.setup({
  timeout: '600-1000',
});
