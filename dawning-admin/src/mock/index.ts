import Mock from 'mockjs';

// 禁用所有 mock，使用真实后端 API
// import './user';
import './message-box';

// 禁用 dashboard mock，如果需要真实数据
// import '@/views/dashboard/workplace/mock';

Mock.setup({
  timeout: '600-1000',
});
