import styled from 'styled-components';

const Panel = styled.div`
  background-color: var(--panel-bg);
  border-radius: 8px;
  padding: 20px;
  border: 1px solid var(--border-color);
  margin-bottom: 20px;
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

const IoDetailedGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(8, 1fr);
  gap: 12px;
  padding: 10px;
`;

const IoBit = styled.div`
  background: var(--item-bg);
  padding: 10px 5px;
  border-radius: 6px;
  display: flex;
  flex-direction: column;
  align-items: center;
  font-size: 11px;
  border: 1px solid #eee;
`;

const LedLarge = styled.div<{ active?: boolean }>`
  width: 18px;
  height: 18px;
  border-radius: 50%;
  background: ${props => props.active ? '#2ecc71' : 'var(--led-off)'};
  box-shadow: ${props => props.active ? '0 0 8px rgba(46, 204, 113, 0.6)' : 'none'};
  margin-bottom: 6px;
  border: 1px solid rgba(0,0,0,0.1);
`;

const IoLabel = styled.span`
  color: var(--text-color);
  font-size: 10px;
  font-weight: 700;
`;

interface IOStatusDetailedProps {
  inputs: boolean[];
  outputs: boolean[];
}

const IOStatusDetailed = ({ inputs, outputs }: IOStatusDetailedProps) => {
  return (
    <>
      <Panel>
        <PanelTitle>📥 Digital Inputs (DI Map - 64 bits)</PanelTitle>
        <IoDetailedGrid>
          {inputs.map((active, index) => (
            <IoBit key={`di-${index}`}>
              <LedLarge active={active} />
              <IoLabel>{index}</IoLabel>
            </IoBit>
          ))}
        </IoDetailedGrid>
      </Panel>

      <Panel>
        <PanelTitle>📤 Digital Outputs (DO Map - 64 bits)</PanelTitle>
        <IoDetailedGrid>
          {outputs.map((active, index) => (
            <IoBit key={`do-${index}`}>
              <LedLarge active={active} />
              <IoLabel>{index}</IoLabel>
            </IoBit>
          ))}
        </IoDetailedGrid>
      </Panel>
    </>
  );
};

export default IOStatusDetailed;
