import styled from 'styled-components';

const Panel = styled.div`
  background-color: var(--panel-bg);
  border-radius: 8px;
  padding: 20px;
  border: 1px solid var(--border-color);
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
`;

const PanelTitle = styled.div`
  font-size: 16px;
  font-weight: 700;
  margin-bottom: 15px;
  color: var(--primary-color);
  display: flex;
  align-items: center;
  gap: 8px;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 10px;
`;

const PosTable = styled.table`
  width: 100%;
  border-collapse: collapse;
  table-layout: fixed;
`;

const Th = styled.th<{ align?: string }>`
  text-align: ${props => props.align || 'right'};
  color: var(--text-muted);
  font-size: 12px;
  padding: 8px 0;
  font-weight: 600;
`;

const Td = styled.td`
  padding: 12px 5px;
  border-bottom: 1px solid var(--item-bg);
`;

const AxisLabel = styled.span`
  font-weight: 800;
  font-size: 22px;
  color: var(--primary-color);
  display: block;
  text-align: left;
  padding-left: 5px;
`;

const PosValue = styled.span<{ type?: 'work' }>`
  font-family: 'Consolas', 'Courier New', monospace;
  font-size: 26px;
  color: ${props => props.type === 'work' ? 'var(--pos-work)' : 'var(--pos-machine)'};
  display: block;
  text-align: right;
  font-weight: 700;
`;

const ButtonGroup = styled.div`
  margin-top: 20px;
  display: flex;
  gap: 10px;
`;

const ActionButton = styled.button<{ variant?: 'primary' | 'danger' }>`
  flex: 1;
  background-color: ${props => 
    props.variant === 'primary' ? 'var(--primary-color)' : 
    props.variant === 'danger' ? 'var(--error-color)' : '#333'};
  color: white;
  padding: 12px;
  border-radius: 4px;
  font-weight: bold;
  &:hover { opacity: 0.9; }
`;

interface AxisMonitorProps {
  positions: {
    axis: string;
    g53: number;
    g54: number;
  }[];
}

const AxisMonitor = ({ positions }: AxisMonitorProps) => {
  return (
    <Panel>
      <PanelTitle>📡 Real-time Position (G53 / G54)</PanelTitle>
      <PosTable>
        <colgroup>
          <col style={{ width: '15%' }} />
          <col style={{ width: '42.5%' }} />
          <col style={{ width: '42.5%' }} />
        </colgroup>
        <thead>
          <tr>
            <Th align="left" style={{ paddingLeft: '10px' }}>AXIS</Th>
            <Th>G53 (MACHINE)</Th>
            <Th>G54 (WORK)</Th>
          </tr>
        </thead>
        <tbody>
          {positions.map((p) => (
            <tr key={p.axis}>
              <Td><AxisLabel>{p.axis}</AxisLabel></Td>
              <Td><PosValue>{p.g53.toFixed(3)}</PosValue></Td>
              <Td><PosValue type="work">{p.g54.toFixed(3)}</PosValue></Td>
            </tr>
          ))}
        </tbody>
      </PosTable>
      <ButtonGroup>
        <ActionButton variant="primary">SERVO ON/OFF</ActionButton>
        <ActionButton>HOME ALL</ActionButton>
        <ActionButton variant="danger">EMG STOP</ActionButton>
      </ButtonGroup>
    </Panel>
  );
};

export default AxisMonitor;
