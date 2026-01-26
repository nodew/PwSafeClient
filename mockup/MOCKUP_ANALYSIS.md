# PwSafe Mockup Design Analysis & Improvement Plan

## 概述 (Overview)

本文档详细分析了当前PwSafe Mobile应用Mockup设计中存在的所有不合理之处，包括UI/UX体验、逻辑流程、交互问题、一致性问题等，并为每个问题提出具体的改进方案。

This document provides a comprehensive analysis of all issues found in the current PwSafe Mobile app mockup designs, including UI/UX experience, logic flow, interaction problems, and consistency issues, with specific improvement solutions for each problem.

---

## 目录 (Table of Contents)

1. [设计一致性问题 (Design Consistency Issues)](#1-设计一致性问题)
2. [颜色方案不统一 (Inconsistent Color Schemes)](#2-颜色方案不统一)
3. [字体和排版问题 (Typography Issues)](#3-字体和排版问题)
4. [交互体验问题 (Interaction Experience Issues)](#4-交互体验问题)
5. [信息架构问题 (Information Architecture Issues)](#5-信息架构问题)
6. [可访问性问题 (Accessibility Issues)](#6-可访问性问题)
7. [安全性和用户体验平衡 (Security & UX Balance)](#7-安全性和用户体验平衡)
8. [响应式设计问题 (Responsive Design Issues)](#8-响应式设计问题)
9. [状态反馈问题 (State Feedback Issues)](#9-状态反馈问题)
10. [导航流程问题 (Navigation Flow Issues)](#10-导航流程问题)

---

## 1. 设计一致性问题 (Design Consistency Issues)

### 问题 1.1: 主题色不统一
**表现:**
- `database_list.html` 使用 `#1773cf` (蓝色)
- `master_password_entry.html` 使用 `#1773cf`
- `settings_database.html` 使用 `#007AFF` (iOS标准蓝)
- `create_new_database.html` 使用 `#007AFF`
- 存在两种不同的主题蓝色

**影响:**
- 破坏品牌一致性
- 用户体验不连贯
- 开发时容易出错

**解决方案:**
```css
/* 统一使用一种主题色 */
--primary: #1773cf; /* 或选择 #007AFF，但必须统一 */
```
**实施建议:** 在所有mockup中使用相同的primary颜色值，建议使用 `#1773cf` 作为品牌色，或者如果要符合iOS设计规范，统一使用 `#007AFF`。

---

### 问题 1.2: 圆角半径不一致
**表现:**
- 部分页面使用 `0.25rem` (4px)
- 部分页面使用 `0.5rem` (8px)
- 部分页面使用 `0.75rem` (12px)
- 按钮、卡片、输入框的圆角各不相同

**影响:**
- 视觉风格不统一
- 缺乏专业感

**解决方案:**
```css
/* 建立统一的圆角系统 */
--radius-sm: 0.5rem;   /* 8px - 小元素（按钮、徽章） */
--radius-md: 0.75rem;  /* 12px - 中等元素（输入框、卡片） */
--radius-lg: 1rem;     /* 16px - 大元素（模态框、面板） */
--radius-full: 9999px; /* 完全圆形 */
```

---

### 问题 1.3: 图标库混用
**表现:**
- 大部分页面使用 Material Symbols Outlined
- `settings_database.html` 和 `settings_system.html` 使用 Material Icons Outlined
- 不同的图标字体可能导致样式细微差异

**影响:**
- 图标风格不一致
- 加载多个图标库影响性能
- 维护困难

**解决方案:**
- 统一使用 `Material Symbols Outlined`
- 更新所有页面引用同一图标库
- 确保图标大小和权重设置一致

---

### 问题 1.4: 字体家族不统一
**表现:**
- 大部分页面使用 `Manrope`
- `master_password_entry.html` 同时引入 `Noto Sans`
- `settings` 页面使用 `Inter`

**影响:**
- 加载多个字体影响性能
- 文本渲染不一致

**解决方案:**
```css
/* 统一字体系统 */
--font-display: 'Manrope', sans-serif;
--font-mono: 'SF Mono', 'Monaco', 'Inconsolata', monospace; /* 用于密码显示 */
```

---

## 2. 颜色方案不统一 (Inconsistent Color Schemes)

### 问题 2.1: 背景色命名和值不一致
**表现:**
```css
/* database_list.html */
"background-light": "#F3F4F6"

/* password_list.html */
"background-light": "#f6f7f8"

/* master_password_entry.html */
"background-light": "#f8fafc"

/* create_new_database.html */
"background-light": "#F2F2F7"
```

**影响:**
- 页面间过渡视觉跳跃
- 不同页面感觉像不同的应用

**解决方案:**
```css
/* 统一颜色系统 */
:root {
  /* Light Mode */
  --bg-primary: #F2F2F7;      /* 主背景 */
  --bg-secondary: #FFFFFF;     /* 卡片/表面背景 */
  --bg-tertiary: #F6F7F8;      /* 三级背景 */
  
  /* Dark Mode */
  --bg-primary-dark: #000000;
  --bg-secondary-dark: #1C1C1E;
  --bg-tertiary-dark: #2C2C2E;
}
```

---

### 问题 2.2: 文字颜色层级混乱
**表现:**
- 次要文本颜色在不同页面使用不同的值
- 缺乏清晰的文本层级系统

**影响:**
- 信息层级不清晰
- 可读性问题

**解决方案:**
```css
:root {
  /* Text Colors - Light Mode */
  --text-primary: #000000;        /* 主要文本 */
  --text-secondary: #3C3C4399;    /* 次要文本 (60% opacity) */
  --text-tertiary: #3C3C4366;     /* 三级文本 (40% opacity) */
  --text-disabled: #3C3C4333;     /* 禁用文本 (20% opacity) */
  
  /* Text Colors - Dark Mode */
  --text-primary-dark: #FFFFFF;
  --text-secondary-dark: #EBEBF599;
  --text-tertiary-dark: #EBEBF566;
}
```

---

## 3. 字体和排版问题 (Typography Issues)

### 问题 3.1: 文本大小不一致
**表现:**
- 标题大小在不同页面使用不同的值
- 正文文本大小不统一
- 缺乏排版规范

**影响:**
- 视觉层次混乱
- 阅读体验不佳

**解决方案:**
```css
/* 建立排版系统 */
:root {
  /* iOS Human Interface Guidelines 推荐 */
  --text-large-title: 34px;  /* 大标题 */
  --text-title-1: 28px;      /* 标题1 */
  --text-title-2: 22px;      /* 标题2 */
  --text-title-3: 20px;      /* 标题3 */
  --text-headline: 17px;     /* 重点文本 */
  --text-body: 17px;         /* 正文 */
  --text-callout: 16px;      /* 说明文本 */
  --text-subheadline: 15px;  /* 副标题 */
  --text-footnote: 13px;     /* 脚注 */
  --text-caption: 12px;      /* 图片说明 */
}
```

---

### 问题 3.2: 行高和间距不规范
**表现:**
- 文本间距设置不统一
- 某些页面文本过于拥挤
- 缺乏呼吸空间

**解决方案:**
```css
/* 统一行高规范 */
--leading-tight: 1.2;    /* 标题用 */
--leading-normal: 1.5;   /* 正文用 */
--leading-relaxed: 1.75; /* 长文本用 */

/* 统一间距系统 */
--spacing-xs: 0.25rem;   /* 4px */
--spacing-sm: 0.5rem;    /* 8px */
--spacing-md: 1rem;      /* 16px */
--spacing-lg: 1.5rem;    /* 24px */
--spacing-xl: 2rem;      /* 32px */
```

---

## 4. 交互体验问题 (Interaction Experience Issues)

### 问题 4.1: 密码可见性切换按钮位置不一致
**表现:**
- `add_password_entry.html`: 显示"visibility"图标在右侧
- `edit_password_entry.html`: 显示"visibility"图标在右侧
- `master_password_entry.html`: 显示"visibility"图标在右侧
- `change_master_password.html`: 显示"Show"文本按钮
- 样式和交互方式不统一

**影响:**
- 用户需要在不同页面寻找功能
- 学习成本增加

**解决方案:**
- 统一使用图标按钮（visibility/visibility_off）
- 位置统一放在输入框右侧
- 添加统一的hover和active状态

---

### 问题 4.2: 复制功能反馈不一致
**表现:**
- `password_list.html`: 底部显示toast "Password copied"
- `entry_detail.html`: 有复制按钮但未显示反馈
- 缺乏统一的复制成功反馈机制

**影响:**
- 用户不确定操作是否成功
- 体验不连贯

**解决方案:**
- 统一使用底部居中的toast通知
- 包含图标和文字
- 3秒后自动消失
- 添加淡入淡出动画

```html
<!-- 统一Toast组件 -->
<div class="toast" role="alert">
  <span class="material-symbols-outlined">check_circle</span>
  <span>Password copied</span>
</div>
```

---

### 问题 4.3: 长按交互缺失清晰指示
**表现:**
- `password_list_long_press_item.html` 显示长按后的菜单
- 但普通的密码列表页没有视觉提示告知用户可以长按
- 新用户难以发现这个功能

**影响:**
- 功能可发现性差
- 依赖用户偶然发现

**解决方案:**
- 添加首次使用引导提示
- 在列表项上轻微显示"···"菜单图标
- 添加触觉反馈（haptic feedback）
- 在设置页面添加手势说明

---

### 问题 4.4: 按钮状态反馈不足
**表现:**
- 大部分按钮只有hover状态
- 缺少active/pressed状态
- 缺少loading状态
- 缺少disabled状态

**影响:**
- 用户不确定按钮是否可点击
- 加载时用户可能重复点击

**解决方案:**
```css
/* 统一按钮状态系统 */
.button {
  /* Normal state */
  transition: all 0.2s ease;
}

.button:hover {
  /* Hover state */
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.button:active {
  /* Active state */
  transform: scale(0.98);
}

.button:disabled {
  /* Disabled state */
  opacity: 0.5;
  cursor: not-allowed;
}

.button.loading {
  /* Loading state */
  pointer-events: none;
  position: relative;
}
```

---

## 5. 信息架构问题 (Information Architecture Issues)

### 问题 5.1: 面包屑导航不完整
**表现:**
- `password_list.html` 显示 "Home > Root"
- 但进入子文件夹后没有显示完整路径
- 用户难以知道当前位置

**影响:**
- 用户在深层级时容易迷失
- 无法快速跳转到上级目录

**解决方案:**
- 显示完整的层级路径
- 每个层级可点击，快速返回
- 当路径过长时，使用省略号折叠中间部分
```
Home > Banking > Credit Cards > Chase
```

---

### 问题 5.2: 锁定状态显示混乱
**表现:**
- `password_list.html`: 显示 "Unlocked • Root"
- `password_list_create_new.html`: 显示 "Locked • Root"
- 同一个页面在不同mockup中显示不同状态

**影响:**
- 混淆开发人员
- 状态逻辑不清晰

**解决方案:**
- 明确定义锁定/解锁状态的含义
- 统一状态显示方式
- 添加锁定图标作为视觉提示
- 解锁状态应该是常态，不需要显示；只在即将自动锁定时提示

---

### 问题 5.3: Entry类型图标不统一
**表现:**
- `password_list.html`: 密码条目使用 `vpn_key` 图标，笔记使用 `description` 图标
- 但图标颜色和背景样式不一致
- 缺乏清晰的类型区分系统

**影响:**
- 用户难以快速识别条目类型
- 视觉层级不清晰

**解决方案:**
```css
/* 建立统一的条目类型系统 */
.entry-icon {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* 密码条目 */
.entry-icon--password {
  background: rgba(23, 115, 207, 0.1);
  color: #1773cf;
}

/* 安全笔记 */
.entry-icon--note {
  background: rgba(245, 158, 11, 0.1);
  color: #f59e0b;
}

/* 文件夹 */
.entry-icon--folder {
  color: #fbbf24;
}
```

---

## 6. 可访问性问题 (Accessibility Issues)

### 问题 6.1: 颜色对比度不足
**表现:**
- 某些次要文本颜色对比度低于WCAG AA标准
- `text-secondary-light: #6b7280` 在白色背景上对比度可能不足

**影响:**
- 视力障碍用户难以阅读
- 在强光下难以看清

**解决方案:**
- 使用对比度检查工具验证所有文本
- 确保至少达到WCAG AA标准（4.5:1）
- 重要信息达到AAA标准（7:1）

---

### 问题 6.2: 缺少合适的ARIA标签
**表现:**
- 按钮缺少 `aria-label`
- 交互元素缺少适当的role
- 表单输入缺少关联的label

**影响:**
- 屏幕阅读器用户无法理解界面
- 不符合无障碍标准

**解决方案:**
```html
<!-- 添加适当的ARIA标签 -->
<button aria-label="Show password" aria-pressed="false">
  <span class="material-symbols-outlined">visibility</span>
</button>

<button aria-label="Copy password to clipboard">
  <span class="material-symbols-outlined">content_copy</span>
</button>

<!-- 表单输入使用label -->
<label for="password-input" class="sr-only">Password</label>
<input id="password-input" type="password" aria-describedby="password-help">
<p id="password-help" class="text-sm">Password must be at least 8 characters</p>
```

---

### 问题 6.3: 焦点指示器不明显
**表现:**
- 默认的浏览器焦点样式被移除
- 自定义的focus状态不够明显
- 键盘用户难以追踪当前焦点位置

**影响:**
- 键盘导航困难
- 不符合无障碍标准

**解决方案:**
```css
/* 统一的焦点指示器 */
*:focus-visible {
  outline: 2px solid var(--primary);
  outline-offset: 2px;
  border-radius: 4px;
}

/* 对于某些元素使用ring样式 */
.input:focus-visible {
  outline: none;
  box-shadow: 0 0 0 3px rgba(23, 115, 207, 0.3);
}
```

---

### 问题 6.4: 最小触摸目标尺寸不达标
**表现:**
- 某些图标按钮小于44x44px
- 不符合iOS和Android的最小触摸目标要求

**影响:**
- 难以精确点击
- 特别影响手指较大或运动障碍用户

**解决方案:**
```css
/* 确保所有可交互元素至少44x44px */
.button, .icon-button {
  min-width: 44px;
  min-height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
}
```

---

## 7. 安全性和用户体验平衡 (Security & UX Balance)

### 问题 7.1: 密码强度指示器不统一
**表现:**
- `add_password_entry.html`: 使用渐变色条 + "Medium"文本
- `edit_password_entry.html`: 使用分段色条 + "Strong"文本
- `entry_detail.html`: 使用渐变色条 + "Strong"文本
- 样式和表现方式不一致

**影响:**
- 用户对密码强度的理解不一致
- 缺乏明确的安全指导

**解决方案:**
```html
<!-- 统一的密码强度指示器 -->
<div class="password-strength">
  <div class="strength-bars">
    <div class="bar" data-level="weak"></div>
    <div class="bar" data-level="fair"></div>
    <div class="bar" data-level="good"></div>
    <div class="bar" data-level="strong"></div>
  </div>
  <span class="strength-label" data-strength="strong">Strong</span>
</div>

<style>
  .strength-bars {
    display: flex;
    gap: 4px;
    height: 4px;
  }
  
  .bar {
    flex: 1;
    background: #E5E7EB;
    border-radius: 2px;
    transition: background 0.3s;
  }
  
  /* Weak (1 bar) */
  [data-strength="weak"] .bar:nth-child(1) { background: #EF4444; }
  
  /* Fair (2 bars) */
  [data-strength="fair"] .bar:nth-child(-n+2) { background: #F59E0B; }
  
  /* Good (3 bars) */
  [data-strength="good"] .bar:nth-child(-n+3) { background: #10B981; }
  
  /* Strong (4 bars) */
  [data-strength="strong"] .bar { background: #059669; }
</style>
```

---

### 问题 7.2: 生成密码功能位置不一致
**表现:**
- `add_password_entry.html`: 密码输入框右侧有"casino"图标
- `edit_password_entry.html`: 密码输入框右侧有"autorenew"图标，下方还有独立的生成按钮
- 同一功能使用不同图标和位置

**影响:**
- 用户困惑
- 功能重复

**解决方案:**
- 统一使用 `autorenew` 图标表示生成密码
- 位置统一在密码输入框右侧
- 点击后显示密码生成选项对话框（长度、字符类型等）

---

### 问题 7.3: 关键操作缺少二次确认
**表现:**
- `edit_password_entry.html`: "Delete Entry"按钮直接删除
- `entry_detail.html`: "Delete"按钮没有确认对话框
- 危险操作缺少保护机制

**影响:**
- 用户可能误操作导致数据丢失
- 无法撤销的操作需要额外保护

**解决方案:**
```html
<!-- 删除前显示确认对话框 -->
<div class="alert-dialog">
  <h3>Delete Entry</h3>
  <p>Are you sure you want to delete "Netflix"? This action cannot be undone.</p>
  <div class="actions">
    <button class="button--secondary">Cancel</button>
    <button class="button--danger">Delete</button>
  </div>
</div>
```

---

## 8. 响应式设计问题 (Responsive Design Issues)

### 问题 8.1: 固定宽度限制
**表现:**
- 所有mockup使用 `max-w-[480px]`
- 在更大的屏幕上浪费空间
- 在更小的屏幕上可能溢出

**影响:**
- 不能充分利用平板设备的屏幕
- 极小屏幕设备可能显示不正常

**解决方案:**
```css
/* 响应式容器 */
.app-container {
  width: 100%;
  max-width: 480px;  /* 手机 */
  margin: 0 auto;
}

@media (min-width: 768px) {
  /* 平板竖屏 */
  .app-container {
    max-width: 600px;
  }
}

@media (min-width: 1024px) and (orientation: landscape) {
  /* 平板横屏 */
  .app-container {
    max-width: 800px;
  }
}
```

---

### 问题 8.2: 文本截断不友好
**表现:**
- 长文本使用 `truncate` 但没有提供查看完整文本的方式
- 用户无法看到完整的密码标题或用户名

**影响:**
- 信息丢失
- 难以区分相似条目

**解决方案:**
- 添加tooltip显示完整文本
- 或使用多行显示重要信息
```html
<p class="truncate" title="This is the full text that will appear in tooltip">
  This is the full text...
</p>
```

---

## 9. 状态反馈问题 (State Feedback Issues)

### 问题 9.1: 加载状态缺失
**表现:**
- 所有页面都缺少加载状态
- 解锁数据库、保存条目等操作没有加载指示

**影响:**
- 用户不知道操作是否在进行
- 可能重复点击按钮

**解决方案:**
```html
<!-- 添加加载状态 -->
<button class="button loading" disabled>
  <span class="spinner"></span>
  <span>Unlocking...</span>
</button>

<style>
  .spinner {
    width: 16px;
    height: 16px;
    border: 2px solid rgba(255,255,255,0.3);
    border-top-color: white;
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
  }
  
  @keyframes spin {
    to { transform: rotate(360deg); }
  }
</style>
```

---

### 问题 9.2: 错误状态处理不完整
**表现:**
- 没有显示错误提示的mockup
- 缺少表单验证错误样式
- 网络错误、认证失败等情况没有考虑

**影响:**
- 开发时缺少参考
- 用户遇到错误时不知道如何处理

**解决方案:**
```html
<!-- 表单错误状态 */
<div class="form-field error">
  <label for="password">Password</label>
  <input id="password" type="password" aria-invalid="true" aria-describedby="password-error">
  <p id="password-error" class="error-message">
    <span class="material-symbols-outlined">error</span>
    Incorrect password. Please try again.
  </p>
</div>

<style>
  .form-field.error input {
    border-color: #EF4444;
  }
  
  .error-message {
    color: #EF4444;
    font-size: 14px;
    margin-top: 4px;
    display: flex;
    align-items: center;
    gap: 4px;
  }
</style>
```

---

### 问题 9.3: 空状态缺失
**表现:**
- 没有显示空密码列表的mockup
- 没有显示搜索无结果的状态
- 新用户首次使用时缺少引导

**影响:**
- 新用户不知道如何开始
- 空状态时用户感到困惑

**解决方案:**
```html
<!-- 空状态 -->
<div class="empty-state">
  <div class="empty-icon">
    <span class="material-symbols-outlined">folder_open</span>
  </div>
  <h3>No passwords yet</h3>
  <p>Tap the + button to add your first password</p>
  <button class="button--primary">Add Password</button>
</div>
```

---

## 10. 导航流程问题 (Navigation Flow Issues)

### 问题 10.1: 返回按钮不一致
**表现:**
- `password_list.html`: 使用 `arrow_back_ios_new` 图标
- `settings_database.html`: 使用 `arrow_back_ios_new` 图标
- `entry_detail.html`: 使用 `arrow_back_ios_new` 图标
- `add_password_entry.html`: 使用文本"Cancel"
- 部分使用图标，部分使用文本

**影响:**
- 导航方式不统一
- 用户需要适应不同的返回方式

**解决方案:**
- 统一规则：
  - 模态页面（全屏覆盖）使用"Cancel"文本
  - 常规页面导航使用返回图标
  - 或者统一都使用返回图标 + 返回目标文本（如"< Back"）

---

### 问题 10.2: 底部FAB按钮与底部导航冲突
**表现:**
- `password_list.html` 有右下角的FAB（+）按钮
- 但没有显示底部导航栏
- 实际应用中底部通常有导航栏，FAB可能遮挡

**影响:**
- FAB可能遮挡底部导航
- 单手操作不便

**解决方案:**
```css
/* 如果有底部导航栏，调整FAB位置 */
.fab {
  position: fixed;
  bottom: 80px; /* 为底部导航栏留出空间 */
  right: 16px;
}

/* 或使用header中的+按钮 */
```

---

### 问题 10.3: 设置页面分离不清晰
**表现:**
- `settings_system.html` (系统设置) 和 `settings_database.html` (数据库设置) 内容有重叠
- 用户难以理解为什么有两个设置页面
- 导航逻辑不清晰

**影响:**
- 用户困惑
- 设置项难以找到

**解决方案:**
- 明确划分：
  - **系统设置**：语言、外观、生物识别、自动锁定、剪贴板清除等全局设置
  - **数据库设置**：当前数据库相关的设置，如重命名、更改主密码、备份、导出等
- 在未打开数据库时，只能访问系统设置
- 打开数据库后，设置菜单应显示两个选项或使用标签页

---

## 总结与优先级建议 (Summary & Priority Recommendations)

### 高优先级 (High Priority)
应立即修复的问题，影响核心用户体验和品牌一致性：

1. **统一主题色和颜色系统** - 建立一致的设计语言
2. **统一字体和排版** - 提升专业度和可读性
3. **统一图标库** - 减少加载时间，保持视觉一致性
4. **修复密码可见性切换** - 核心功能的一致性
5. **添加加载和错误状态** - 基本的状态反馈

### 中优先级 (Medium Priority)
应在下一迭代中修复的问题：

1. **改进面包屑导航** - 提升可用性
2. **统一密码强度指示器** - 安全性提示的一致性
3. **添加危险操作确认** - 防止误操作
4. **改进空状态设计** - 新用户引导
5. **优化响应式设计** - 适配更多设备

### 低优先级 (Low Priority)
可以在后续版本中逐步改进：

1. **完善无障碍支持** - ARIA标签、焦点管理等
2. **优化动画效果** - 微交互和过渡动画
3. **添加高级功能提示** - 如长按操作的发现性
4. **优化深色模式** - 完善深色模式的一致性

---

## 实施建议 (Implementation Recommendations)

### 阶段一：建立设计系统
1. 创建 `design-system.css` 文件
2. 定义所有设计令牌（颜色、字体、间距、圆角等）
3. 更新所有mockup使用统一的设计系统

### 阶段二：组件标准化
1. 创建可复用的组件库
2. 统一按钮、输入框、卡片等基础组件
3. 建立组件使用规范文档

### 阶段三：交互细节优化
1. 添加状态变化（hover、active、disabled、loading）
2. 完善反馈机制（toast、alert、validation）
3. 优化动画和过渡效果

### 阶段四：无障碍和优化
1. 添加完整的ARIA支持
2. 优化键盘导航
3. 进行可访问性测试

---

## 附录：设计系统代码模板 (Appendix: Design System Code Template)

```css
/* PwSafe Design System */

:root {
  /* === Colors === */
  /* Primary */
  --color-primary: #1773cf;
  --color-primary-hover: #1460ab;
  --color-primary-active: #0f4d8a;
  
  /* Background */
  --color-bg-primary: #F2F2F7;
  --color-bg-secondary: #FFFFFF;
  --color-bg-tertiary: #F6F7F8;
  
  /* Text */
  --color-text-primary: #000000;
  --color-text-secondary: rgba(60, 60, 67, 0.6);
  --color-text-tertiary: rgba(60, 60, 67, 0.3);
  
  /* Semantic Colors */
  --color-success: #10B981;
  --color-warning: #F59E0B;
  --color-error: #EF4444;
  --color-info: #3B82F6;
  
  /* === Typography === */
  --font-family: 'Manrope', -apple-system, BlinkMacSystemFont, sans-serif;
  --font-family-mono: 'SF Mono', Monaco, 'Courier New', monospace;
  
  --font-size-xs: 0.75rem;    /* 12px */
  --font-size-sm: 0.875rem;   /* 14px */
  --font-size-base: 1rem;     /* 16px */
  --font-size-lg: 1.125rem;   /* 18px */
  --font-size-xl: 1.25rem;    /* 20px */
  --font-size-2xl: 1.5rem;    /* 24px */
  
  --font-weight-normal: 400;
  --font-weight-medium: 500;
  --font-weight-semibold: 600;
  --font-weight-bold: 700;
  
  --line-height-tight: 1.2;
  --line-height-normal: 1.5;
  --line-height-relaxed: 1.75;
  
  /* === Spacing === */
  --spacing-1: 0.25rem;  /* 4px */
  --spacing-2: 0.5rem;   /* 8px */
  --spacing-3: 0.75rem;  /* 12px */
  --spacing-4: 1rem;     /* 16px */
  --spacing-5: 1.25rem;  /* 20px */
  --spacing-6: 1.5rem;   /* 24px */
  --spacing-8: 2rem;     /* 32px */
  
  /* === Border Radius === */
  --radius-sm: 0.5rem;   /* 8px */
  --radius-md: 0.75rem;  /* 12px */
  --radius-lg: 1rem;     /* 16px */
  --radius-xl: 1.5rem;   /* 24px */
  --radius-full: 9999px;
  
  /* === Shadows === */
  --shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
  --shadow-xl: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
  
  /* === Transitions === */
  --transition-fast: 150ms ease;
  --transition-base: 200ms ease;
  --transition-slow: 300ms ease;
}

/* Dark Mode Overrides */
@media (prefers-color-scheme: dark) {
  :root {
    --color-bg-primary: #000000;
    --color-bg-secondary: #1C1C1E;
    --color-bg-tertiary: #2C2C2E;
    
    --color-text-primary: #FFFFFF;
    --color-text-secondary: rgba(235, 235, 245, 0.6);
    --color-text-tertiary: rgba(235, 235, 245, 0.3);
  }
}
```

---

**文档版本:** 1.0  
**最后更新:** 2024-01-26  
**维护者:** PwSafe Design Team
