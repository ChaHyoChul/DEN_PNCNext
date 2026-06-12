import { createGlobalStyle } from 'styled-components';

const GlobalStyle = createGlobalStyle`
  :root {
    --bg-color: #ffffff;
    --primary-color: #2c3e50;
    --secondary-color: #34495e;
    --accent-color: #3498db;
    --success-color: #27ae60;
    --error-color: #c0392b;
    --warning-color: #d35400;
    --text-color: #222222;
    --text-muted: #666666;
    --border-color: #cccccc;
    
    /* Motion Specific Light Theme */
    --panel-bg: #ffffff;
    --item-bg: #f8f9fa;
    --pos-machine: #1b5e20; /* Deep Green */
    --pos-work: #0d47a1;    /* Deep Blue */
    --led-off: #dee2e6;
  }

  * {
    box-sizing: border-box;
    margin: 0;
    padding: 0;
  }

  body {
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    background-color: var(--bg-color);
    color: var(--text-color);
    line-height: 1.6;
  }

  button {
    cursor: pointer;
    border: none;
    outline: none;
    font-family: inherit;
  }

  ul {
    list-style: none;
  }

  h1, h2, h3, h4 {
    color: var(--primary-color);
  }
`;

export default GlobalStyle;
